using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Data.Migrations;

public partial class AddCivilTopicSelection : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ArticleNumbers",
            table: "StudyTopics",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "SelectedTopicKeys",
            table: "StudyTopics",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ArticleNumbers", table: "StudyTopics");
        migrationBuilder.DropColumn(name: "SelectedTopicKeys", table: "StudyTopics");
    }
}
