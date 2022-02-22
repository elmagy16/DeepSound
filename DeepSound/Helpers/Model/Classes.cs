namespace DeepSound.Helpers.Model
{
    public class Classes
    {
        public class LibraryItem
        {
            public string SectionId { get; set; }
            public string SectionText { get; set; }
            public int SongsCount { get; set; }
            public string BackgroundImage { get; set; }
        }
         
        public class Categories
        {
            public string CategoriesId { get; set; }
            public string CategoriesName { get; set; }
            public string CategoriesColor { get; set; }
            public string CategoriesIcon { get; set; } 
        }


    }

    public enum ShowAds
    {
        AllUsers,
        UnProfessional,
    }

}