using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using profescipta_test.Controllers;
using profescipta_test.Models;
using profescipta_test.Repository;
using System;
using System.Threading.Tasks;

namespace profescipta_test.Controllers;

public class HomeController : Controller
{

    private SalesOrderRepo _databaseHelper;

    public HomeController(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _databaseHelper = new SalesOrderRepo(connectionString);
    }

  
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }



}

