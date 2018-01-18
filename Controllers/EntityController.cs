using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XMLParser.Models;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using XMLParser.CodeTemplates;
using System.Data.Entity.Migrations;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.CodeDom.Compiler;
using System.Web.Script.Serialization;
using System.Data.Entity;
using System.Xml.Schema;

namespace XMLParser.Controllers
{
    public class EntityController : Controller
    {
        EntityContext db = new EntityContext();

        public ActionResult Index()
        {
            var entities = db.Entity.OrderByDescending(e => e.LastUpdateDate).ToList();

            if(TempData["Success"] != null)
            {
                ViewBag.Success = true;
            }

            return View(entities);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection form)
        {
            Dictionary<string, Dictionary<string, string>> propertyMap = new Dictionary<string, Dictionary<string, string>>();
            string entityName = "";
            string xmlInput = Convert.ToString(form["xml-input"]);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(null, Server.MapPath("~/Content/XSD/schema.xsd"));

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlInput), settings))
                {
                    reader.ReadToFollowing("entity");
                    reader.MoveToAttribute("name");
                    entityName = reader.Value;

                    while (reader.ReadToFollowing("property"))
                    {
                        Dictionary<string, string> attrMap = new Dictionary<string, string>();

                        for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
                        {
                            reader.MoveToAttribute(attInd);
                            string value;

                            if (reader.Name == "name")
                            {
                                value = reader.Value.First().ToString().ToUpper() + reader.Value.Substring(1);
                            }
                            else
                            {
                                value = reader.Value;
                            }

                            attrMap.Add(reader.Name, value);
                        }

                        propertyMap.Add(reader.GetAttribute("name"), attrMap);
                    }
                }

                entityName = entityName.First().ToString().ToUpper() + entityName.Substring(1);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string jsonSchema = serializer.Serialize(propertyMap);

                if (ModelState.IsValid)
                {
                    Entity newEntity = new Entity();
                    newEntity.Name = entityName;
                    newEntity.Schema = jsonSchema;

                    db.Entity.Add(newEntity);
                    db.SaveChanges();
                }
                else
                {
                    ViewBag.Error = "Something went wrong with database. Try later.";
                    ViewBag.Input = xmlInput;
                    return View();
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = "Your XML code is no valid. Change a few things up and try submitting again.";
                ViewBag.Input = xmlInput;
                return View();
            }

            TempData["Success"] = true;
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var data = db.EntityData.Where(e => e.EntityID == id).ToList();
            Dictionary<int, Dictionary<string, string>> items = new Dictionary<int, Dictionary<string, string>>();

            ViewBag.EntityName = db.Entity.SingleOrDefault(e => e.Id == id).Name;
            ViewBag.EntityId = id;

            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"];
            }

            JavaScriptSerializer deserializer = new JavaScriptSerializer();

            foreach (EntityData element in data)
            {
                string json = element.Data;
                Dictionary<string, string> dict = deserializer.Deserialize<Dictionary<string, string>>(json);
                items.Add(element.PropertyID, dict);
            }

            return View(items);
        }

        public JsonResult Delete(int id)
        {
            Entity entity = db.Entity.Find(id);

            if(entity == null)
            {
                return Json(new { message = "Entity not found" });
            }

            db.Entity.Remove(entity);
            var entityData = db.EntityData.Where(e => e.EntityID == id).ToList();

            foreach(var data in entityData)
            {
                db.EntityData.Remove(data);
            }

            db.SaveChanges();

            return Json(new { });
        }

        public JsonResult Rename(int id)
        {
            Entity entity = db.Entity.Find(id);

            if (entity == null)
            {
                return Json(new { message = "Entity not found" });
            }

            entity.Name = Request["name"];
            entity.LastUpdateDate = DateTime.Now;
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { });
        }

        public PartialViewResult Search()
        {
            string input = Request["input"];
            var results = db.Entity.Where(e => e.Name.StartsWith(input)).OrderByDescending(e => e.LastUpdateDate).ToList();

            return PartialView("List", results);
        }

        public ActionResult Documentation()
        {
            return View();
        }
    }
}