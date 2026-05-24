using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogAPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedGames : Migration
    {
        private static readonly DateTime SeedDate = new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "Title", "PriceCents", "Currency", "CreatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "The Witcher 3: Wild Hunt",         7499,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "Red Dead Redemption 2",            9999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "Cyberpunk 2077",                   8999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "God of War",                       8999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "Elden Ring",                      11999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), "Grand Theft Auto V",               5999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), "Minecraft",                        7999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), "Dark Souls III",                   8999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), "Hollow Knight",                    2999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000010"), "Hades",                            4499,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000011"), "Stardew Valley",                   2299,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000012"), "Sekiro: Shadows Die Twice",       11999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000013"), "DOOM Eternal",                    6999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000014"), "Horizon Zero Dawn",               7999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000015"), "Batman: Arkham Knight",           4999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000016"), "Assassin's Creed Valhalla",       8999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000017"), "Control",                         5999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000018"), "Death Stranding",                 7499,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000019"), "Ori and the Will of the Wisps",   3999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000020"), "Celeste",                         1999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000021"), "Disco Elysium",                   5999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000022"), "Divinity: Original Sin 2",        7999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000023"), "Monster Hunter: World",           7999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000024"), "Baldur's Gate 3",                11999,  "BRL", SeedDate },
                    { new Guid("a1000000-0000-0000-0000-000000000025"), "Persona 5 Royal",                 9999,  "BRL", SeedDate },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int i = 1; i <= 25; i++)
            {
                migrationBuilder.DeleteData(
                    table: "Games",
                    keyColumn: "Id",
                    keyValue: new Guid($"a1000000-0000-0000-0000-{i:D12}")
                );
            }
        }
    }
}
