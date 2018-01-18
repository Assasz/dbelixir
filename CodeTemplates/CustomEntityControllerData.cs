namespace XMLParser.CodeTemplates
{
    public partial class CustomEntityController
    {
        public string entityName { get; set; }

        public CustomEntityController(string entityName)
        {
            this.entityName = entityName;
        }
    }
}