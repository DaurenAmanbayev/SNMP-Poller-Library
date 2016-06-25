namespace InventoryModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Vendors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataKeys",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Vendor_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Vendors", t => t.Vendor_Id)
                .Index(t => t.Vendor_Id);
            
            CreateTable(
                "dbo.Nodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                        Credential_Id = c.Int(),
                        Vendor_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Credentials", t => t.Credential_Id)
                .ForeignKey("dbo.Vendors", t => t.Vendor_Id)
                .Index(t => t.Credential_Id)
                .Index(t => t.Vendor_Id);
            
            CreateTable(
                "dbo.Credentials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoCommunity = c.String(),
                        RwCommunity = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Details",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Properties = c.String(),
                        DataKey_Id = c.Int(),
                        Node_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataKeys", t => t.DataKey_Id)
                .ForeignKey("dbo.Nodes", t => t.Node_Id)
                .Index(t => t.DataKey_Id)
                .Index(t => t.Node_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Nodes", "Vendor_Id", "dbo.Vendors");
            DropForeignKey("dbo.Details", "Node_Id", "dbo.Nodes");
            DropForeignKey("dbo.Details", "DataKey_Id", "dbo.DataKeys");
            DropForeignKey("dbo.Nodes", "Credential_Id", "dbo.Credentials");
            DropForeignKey("dbo.DataKeys", "Vendor_Id", "dbo.Vendors");
            DropIndex("dbo.Details", new[] { "Node_Id" });
            DropIndex("dbo.Details", new[] { "DataKey_Id" });
            DropIndex("dbo.Nodes", new[] { "Vendor_Id" });
            DropIndex("dbo.Nodes", new[] { "Credential_Id" });
            DropIndex("dbo.DataKeys", new[] { "Vendor_Id" });
            DropTable("dbo.Details");
            DropTable("dbo.Credentials");
            DropTable("dbo.Nodes");
            DropTable("dbo.DataKeys");
            DropTable("dbo.Vendors");
        }
    }
}
