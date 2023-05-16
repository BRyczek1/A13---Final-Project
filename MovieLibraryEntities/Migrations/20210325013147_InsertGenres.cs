using Microsoft.EntityFrameworkCore.Migrations;

namespace MovieLibraryOO.Migrations
{
    public partial class InsertGenres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "Data", @"1-InsertGenres.sql");
            migrationBuilder.Sql(File.ReadAllText(sqlFile));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from genres");
        }
    }
}
