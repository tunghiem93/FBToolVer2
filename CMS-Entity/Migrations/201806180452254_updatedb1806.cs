namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb1806 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CMS_GroupKey", "UpdatedDate", c => c.DateTime());
            DropColumn("dbo.CMS_GroupKey", "UpdatedDated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CMS_GroupKey", "UpdatedDated", c => c.DateTime());
            DropColumn("dbo.CMS_GroupKey", "UpdatedDate");
        }
    }
}
