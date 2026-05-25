using AGM_API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AGM_API.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260517230000_AddFarmTypeAndDetails")]
    partial class AddFarmTypeAndDetails
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder) { }
    }
}
