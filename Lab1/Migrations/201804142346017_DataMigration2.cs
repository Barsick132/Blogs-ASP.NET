namespace Lab1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataMigration2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Articles", "AuthorID", c => c.String(nullable: false));
            AlterColumn("dbo.Articles", "Subject", c => c.String(nullable: false));
            AlterColumn("dbo.Articles", "Text", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Articles", "Text", c => c.String());
            AlterColumn("dbo.Articles", "Subject", c => c.String());
            AlterColumn("dbo.Articles", "AuthorID", c => c.String());
        }
    }
}
