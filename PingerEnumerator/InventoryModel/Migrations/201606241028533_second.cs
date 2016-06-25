namespace InventoryModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class second : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataKeys", "Vendor_Id", "dbo.Vendors");
            DropIndex("dbo.DataKeys", new[] { "Vendor_Id" });
            DropColumn("dbo.DataKeys", "Vendor_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataKeys", "Vendor_Id", c => c.Int());
            CreateIndex("dbo.DataKeys", "Vendor_Id");
            AddForeignKey("dbo.DataKeys", "Vendor_Id", "dbo.Vendors", "Id");
        }
    }
}
