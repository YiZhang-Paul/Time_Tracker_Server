using Core.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CreateEventHistorySummaryView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var baseFields = @"e.""Id"", e.""ResourceId"", e.""EventType"", e.""Timestamp""";

            migrationBuilder.Sql($@"
                CREATE VIEW ""EventHistorySummary"" AS

                SELECT {baseFields}, NULL::character varying AS ""Name"", false AS ""IsDeleted"", false AS ""IsResolved""
                FROM ""EventHistory"" e
                WHERE e.""EventType"" <> {(int)EventType.Interruption} AND e.""EventType"" <> {(int)EventType.Task}

                UNION ALL

                SELECT {baseFields},
                        i.""Name"",
                        i.""IsDeleted"",
                        CASE
                            WHEN i.""ResolvedTime"" IS NOT NULL THEN true
                            ELSE false
                        END AS ""IsResolved""
                FROM ""EventHistory"" e
                JOIN ""InterruptionItem"" i ON e.""EventType"" = {(int)EventType.Interruption} AND e.""ResourceId"" = i.""Id""

                UNION ALL

                SELECT {baseFields},
                        t.""Name"",
                        t.""IsDeleted"",
                        CASE
                            WHEN t.""ResolvedTime"" IS NOT NULL THEN true
                            ELSE false
                        END AS ""IsResolved""
                FROM ""EventHistory"" e
                JOIN ""TaskItem"" t ON e.""EventType"" = {(int)EventType.Task} AND e.""ResourceId"" = t.""Id"";
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""EventHistorySummary"";");
        }
    }
}
