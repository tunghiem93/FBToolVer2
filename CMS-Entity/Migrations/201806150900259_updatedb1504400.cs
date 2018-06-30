namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb1504400 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CMS_Pin", "Link", c => c.String(maxLength: 2000));
            AlterColumn("dbo.CMS_Pin", "ImageUrl", c => c.String(maxLength: 2000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CMS_Pin", "ImageUrl", c => c.String(maxLength: 256));
            AlterColumn("dbo.CMS_Pin", "Link", c => c.String(maxLength: 500));
        }
    }
}
