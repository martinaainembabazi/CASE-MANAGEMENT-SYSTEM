using Microsoft.AspNetCore.Mvc;

public class BreadcrumbViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<(string Title, string Url)> breadcrumbs)
    {
        return View(breadcrumbs);
    }
}