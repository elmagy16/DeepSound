using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using DeepSound.Helpers.Model;
using DeepSound.Helpers.Utils;

namespace DeepSound.Helpers.Controller
{
    public static class CategoriesController
    { 
        public static ObservableCollection<Classes.Categories> ListCategoriesBlog = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesProducts = new ObservableCollection<Classes.Categories>();

        public static string Get_Translate_Categories_Communities(string idCategory, string textCategory , string type)
        {
            try
            {
                string categoryName = textCategory;

                switch (type)
                { 
                    case "Blog":
                    {
                        categoryName = ListCategoriesBlog?.Count switch
                        {
                            > 0 => ListCategoriesBlog.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory,
                            _ => categoryName
                        };

                        break;
                    }
                    case "Products":
                    {
                        categoryName = ListCategoriesProducts?.Count switch
                        {
                            > 0 => ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory,
                            _ => categoryName
                        };

                        break;
                    } 
                    default:
                        categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);
                        break;
                }

                if (string.IsNullOrEmpty(categoryName))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);
                 
                return categoryName; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (string.IsNullOrEmpty(textCategory))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);

                return textCategory;
            }
        } 
    }
}