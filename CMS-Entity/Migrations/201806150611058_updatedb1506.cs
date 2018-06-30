namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb1506 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CMS_Pin", "Link", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CMS_Pin", "Link", c => c.String(maxLength: 256));
        }
    }
}
