using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using XMLParser.Models;
using XMLParser.Services;

namespace XMLParser.Controllers
{
    public class EntityDataController : Controller
    {
        EntityContext db = new EntityContext();

        public ActionResult Create(int entityId)
        {
            var entity = db.Entity.Find(entityId);
            ViewBag.EntityName = entity.Name;
            ViewBag.EntityId = entity.Id;

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(int entityId, FormCollection form)
        {
            Dictionary<string, Dictionary<string, string>> items = new Dictionary<string, Dictionary<string, string>>();
            string xmlInput = Convert.ToString(form["xml-input"]);
            var entity = db.Entity.Find(entityId);
            int lastId = 1;

            if (db.EntityData.Where(e => e.EntityID == entityId).Count() > 0)
            {
                lastId = db.EntityData.Where(e => e.EntityID == entityId).OrderByDescending(e => e.PropertyID).First().PropertyID;
            }

            string error = "";

            JavaScriptSerializer deserializer = new JavaScriptSerializer();
            string json = entity.Schema;
            Dictionary<string, Dictionary<string, string>> schema = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlInput)))
                {
                    while(reader.ReadToFollowing(entity.Name))
                    {
                        Dictionary<string, string> item = new Dictionary<string, string>();

                        foreach(KeyValuePair<string, Dictionary<string, string>> element in schema)
                        {
                            string descendant = element.Key.First().ToString().ToUpper() + element.Key.Substring(1);
                            string defaultValue = "";

                            reader.ReadToFollowing(descendant);

                            if (reader.Name == "")
                            {
                                throw new Exception();
                            }

                            reader.Read();

                            if (element.Value.ContainsKey("nullable"))
                            {
                                if(element.Value["nullable"] == "false" && reader.Value == "")
                                {
                                    error = descendant + " does not accept null.";
                                    throw new Exception();
                                }
                            }

                            if(element.Value.ContainsKey("type"))
                            {
                                TypeValidator validator = new TypeValidator();

                                if (!validator.Validate(reader.Value, element.Value["type"], descendant))
                                {
                                    error = validator.Error;
                                    throw new Exception();
                                }
                            }

                            if(element.Value.ContainsKey("length") && element.Value.ContainsKey("type"))
                            {
                                if (element.Value["type"] == "string")
                                {
                                    if (reader.Value.Length > Convert.ToInt32(element.Value["length"]))
                                    {
                                        error = descendant + " length is too long, " + element.Value["length"] + " chars maximum allowed.";
                                        throw new Exception();
                                    }
                                }
                            }

                            if(element.Value.ContainsKey("default") && reader.Value == "")
                            {
                                defaultValue = element.Value["default"];
                            }

                            item.Add(element.Key, (defaultValue.Length > 0) ? defaultValue : reader.Value);
                        }

                        lastId++;
                        items.Add(Convert.ToString(lastId), item);
                    }
                }

                foreach(KeyValuePair<string, Dictionary<string, string>> item in items)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string jsonSchema = serializer.Serialize(item.Value);

                    if (ModelState.IsValid)
                    {
                        EntityData newItem = new EntityData();
                        newItem.PropertyID = Convert.ToInt32(item.Key);
                        newItem.EntityID = entityId;
                        newItem.Data = jsonSchema;

                        entity.Items += 1;
                        entity.LastUpdateDate = DateTime.Now;

                        db.EntityData.Add(newItem);
                        db.Entry(entity).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.Error = "Something went wrong with database. Try later.";
                        ViewBag.Input = xmlInput;
                        ViewBag.EntityName = entity.Name;
                        ViewBag.EntityId = entity.Id;

                        return View();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = (error.Length > 0) ? error : "Your XML code is no valid. Change a few things up and try submitting again.";
                ViewBag.Input = xmlInput;
                ViewBag.EntityName = entity.Name;
                ViewBag.EntityId = entity.Id;

                return View();
            }

            TempData["Success"] = "create";
            return RedirectToAction("Details", "Entity", new { id = entityId });
        }

        public ActionResult Edit(int entityId, int itemId)
        {
            Entity entity = db.Entity.Find(entityId);
            ViewBag.EntityName = entity.Name;
            ViewBag.EntityId = entity.Id;
            ViewBag.ItemId = itemId;

            EntityData item = db.EntityData.SingleOrDefault(i => i.PropertyID == itemId && i.EntityID == entityId);
            JavaScriptSerializer deserializer = new JavaScriptSerializer();
            string json = item.Data;

            Dictionary<string, string> schema = deserializer.Deserialize<Dictionary<string, string>>(json);

            return View(schema);
        }

        [HttpPost]
        public ActionResult Edit(int entityId, int itemId, FormCollection form)
        {
            EntityData item = db.EntityData.SingleOrDefault(i => i.PropertyID == itemId && i.EntityID == entityId);
            Entity entity = db.Entity.Find(entityId);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonData = item.Data;
            string jsonSchema = entity.Schema;
            string error = "";

            Dictionary<string, string> data = serializer.Deserialize<Dictionary<string, string>>(jsonData);
            Dictionary<string, Dictionary<string, string>> schema = serializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonSchema);
            Dictionary<string, string> newData = new Dictionary<string, string>();

            try
            {
                foreach (KeyValuePair<string, string> element in data)
                {
                    string input = Convert.ToString(form[element.Key]);

                    foreach (KeyValuePair<string, Dictionary<string, string>> property in schema)
                    {
                        if(property.Key != element.Key)
                        {
                            continue;
                        }

                        string propertyName = element.Key.First().ToString().ToUpper() + element.Key.Substring(1);

                        if (property.Value.ContainsKey("nullable"))
                        {
                            if(property.Value["nullable"] == "false" && input == "")
                            {
                                error = propertyName + " cannot be empty.";
                                throw new Exception();
                            }
                        }

                        if (property.Value.ContainsKey("type"))
                        {
                            TypeValidator validator = new TypeValidator();

                            if(!validator.Validate(input, property.Value["type"], propertyName))
                            {
                                error = validator.Error;
                                throw new Exception();
                            }
                        }

                        if (property.Value.ContainsKey("length") && property.Value.ContainsKey("type"))
                        {
                            if (property.Value["type"] == "string")
                            {
                                if (input.Length > Convert.ToInt32(property.Value["length"]))
                                {
                                    error = propertyName + " length is too long, " + property.Value["length"] + " chars maximum allowed.";
                                    throw new Exception();
                                }
                            }
                        }

                        if (property.Value.ContainsKey("default") && input == "")
                        {
                            input = property.Value["default"];
                        }
                    }

                    newData[element.Key] = input;
                }

                string newJsonData = serializer.Serialize(newData);
                item.Data = newJsonData;
                entity.LastUpdateDate = DateTime.Now;

                db.Entry(item).State = EntityState.Modified;
                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "edit";
            }
            catch(Exception e)
            {
                ViewBag.Error = (error.Length > 0) ? error : "Something went wrong! Try submitting again.";
                ViewBag.EntityName = entity.Name;
                ViewBag.EntityId = entity.Id;
                ViewBag.ItemId = itemId;

                return View(data);
            }

            return RedirectToAction("Details", "Entity", new { id = entityId });
        }

        public JsonResult Delete(int entityId, int itemId)
        {
            EntityData item = db.EntityData.SingleOrDefault(i => i.PropertyID == itemId && i.EntityID == entityId);

            if (item == null)
            {
                return Json(new { message = "Item not found" });
            }

            Entity entity = item.Entity;
            entity.Items -= 1;
            entity.LastUpdateDate = DateTime.Now;

            db.EntityData.Remove(item);
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { });
        }

        public PartialViewResult Sort(int entityId)
        {
            string criteria = Request["criteria"];
            var data = db.EntityData.Where(e => e.EntityID == entityId).ToList();

            Dictionary<int, Dictionary<string, string>> items = new Dictionary<int, Dictionary<string, string>>();
            JavaScriptSerializer deserializer = new JavaScriptSerializer();

            foreach (EntityData element in data)
            {
                string json = element.Data;
                Dictionary<string, string> dict = deserializer.Deserialize<Dictionary<string, string>>(json);
                items.Add(element.PropertyID, dict);
            }

            Dictionary<int, Dictionary<string, string>> sorted = new Dictionary<int, Dictionary<string, string>>();
            Dictionary<int, dynamic> criteriaValues = new Dictionary<int, dynamic>();


            foreach (KeyValuePair<int, Dictionary<string, string>> item in items)
            {
                double test;

                if(double.TryParse(item.Value[criteria], out test))
                {
                    double value;
                    value = Convert.ToDouble(item.Value[criteria]);

                    criteriaValues.Add(item.Key, value);
                }
                else
                {
                    criteriaValues.Add(item.Key, item.Value[criteria]);
                }
            }

            List<KeyValuePair<int, dynamic>> list = criteriaValues.ToList();

            if (Request["order"] == "asc")
            {
                list = list.OrderBy(x => x.Value).ToList();
            }
            else
            {
                list = list.OrderByDescending(x => x.Value).ToList();
            }

            foreach (KeyValuePair<int, dynamic> listItem in list)
            {
                sorted.Add(listItem.Key, items[listItem.Key]);
            }

            return PartialView("List", sorted);
        }

        public PartialViewResult Search(int entityId)
        {
            string input = Request["input"];
            var results = db.EntityData.Where(d => d.Data.Contains(input) && d.EntityID == entityId).ToList();

            Dictionary<int, Dictionary<string, string>> items = new Dictionary<int, Dictionary<string, string>>();
            JavaScriptSerializer deserializer = new JavaScriptSerializer();

            ViewBag.EntityId = entityId;

            foreach (EntityData element in results)
            {
                string json = element.Data;
                Dictionary<string, string> dict = deserializer.Deserialize<Dictionary<string, string>>(json);
                items.Add(element.PropertyID, dict);
            }

            return PartialView("List", items);
        }
    }
}
