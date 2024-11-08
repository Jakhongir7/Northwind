using Microsoft.AspNetCore.Mvc;

namespace Northwind.Components
{
    public class BreadcrumbsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string entity, string action = null)
        {
            var model = new BreadcrumbModel
            {
                Entity = entity,
                Action = action == "Create" || action == "Edit" ? action : null
            };

            return View(model);
        }
    }

    public class BreadcrumbModel
    {
        public string Entity { get; set; }
        public string Action { get; set; }
    }
}
