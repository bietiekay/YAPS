using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// Implements the Category related methods like automatic searchterm categories and such
    /// </summary>
    public class CategoryProcessor
    {
        public List<Category> Categories;
        private YAPS.HttpServer internal_http_server_object;

        public CategoryProcessor(YAPS.HttpServer Http_Server_Object)
        {
            Categories = new List<Category>();
            internal_http_server_object = Http_Server_Object;
        }

        #region Automatic Mode Category Handling
        /// <summary>
        /// Tells you in which automatic-mode categories one specific recording is 
        /// </summary>
        /// <param name="recording">the recording to be checked</param>
        /// <returns>a list of categories</returns>
        public List<Category> AutomaticCategoriesForRecording(Recording recording)
        {
            List<Category> rCategoryList = new List<Category>();

            bool found = false;

            // TODO: change this in future versions; category checking is not needed anytime we want to know...just when things change...save the data for future use
            foreach (Category category in Categories)
            {
                // TODO: only if one of the searchterms apply the category is set, change that or make it at least configurable
                // check each category searchterms if it applies
                foreach (String SearchTerm in category.SearchTerms)
                {
                    if (recording.Recording_Name.Contains(SearchTerm))
                    {
                        rCategoryList.Add(category);
                        found = true;
                        break;
                    }
                }                
            }

            if (found) return rCategoryList;
            else
                return null;
        }

        public string RenderCategoryLine(List<Category> categories, char Separator)
        {
            StringBuilder Output = new StringBuilder();

            if (categories == null)
                Output.Append("no category");
            else
            {
                bool morethanone = false;

                // TODO: possible character encoding problem here....
                foreach (Category category in categories)
                {
                    if (!morethanone)
                        Output.Append(category.Name);
                    else
                        Output.Append(Separator+" "+category.Name);
                    morethanone = true;
                }
            }
            return Output.ToString();
        }

        /// <summary>
        /// checks if a recording is in that category or not
        /// </summary>
        /// <param name="recording">Recording to check</param>
        /// <param name="category">if it's in this category</param>
        /// <returns>true if it is, false if not</returns>
        public bool isRecordingInCategory(Recording recording, Category category)
        {
            if (CategoryExists(category.Name))
            {
                // check each category if it applies
                foreach (String SearchTerm in category.SearchTerms)
                {
                    if (recording.Recording_Name.Contains(SearchTerm))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }
        #endregion

        #region Data Managing (Add, Delete, Change...)
        public Category GetCategory(String CategoryName)
        {
            try
            {
                if (CategoryExists(CategoryName))
                {
                    Category newCategory = new Category();
                    // find the object...
                    lock (Categories)
                    {
                        foreach (Category category in Categories)
                        {
                            if (category.Name == CategoryName)
                            {
                                newCategory = category;
                                break;
                            }
                        }
                        // found it, now remove it...
                    }
                    return newCategory;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("GetCategory: " + e.Message);
                return null;
            }
        }

        public bool AddSearchTerm(String CategoryName, String SearchTerm)
        {
            try
            {
                Category workCategory = GetCategory(CategoryName);

                if (workCategory != null)
                {
                    // found it...now check if there is already that particular searchterm   
                    if (workCategory.SearchTerms.Contains(SearchTerm)) return false;

                    workCategory.SearchTerms.Add(SearchTerm);

                    // SavetheSettings
                    internal_http_server_object.Configuration.SaveSettings();

                    return true;
                }
                else
                {
                    // nothing found
                    return false;
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("AddSearchTerm: " + e.Message);
                return false;
            }
        }

        public bool DelSearchTerm(String CategoryName, String SearchTerm)
        {
            try
            {
                Category workCategory = GetCategory(CategoryName);

                if (workCategory != null)
                {
                    // found it...now check if there is already that particular searchterm   
                    if (workCategory.SearchTerms.Contains(SearchTerm))
                    {
                        bool found = workCategory.SearchTerms.Remove(SearchTerm);
                        if (found)
                        {
                            // SavetheSettings
                            internal_http_server_object.Configuration.SaveSettings();
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                {
                    // nothing found
                    return false;
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("DelSearchTerm: " + e.Message);
                return false;
            }
        }

        public bool DelCategory(String CategoryName)
        {
            try
            {
                if (CategoryExists(CategoryName))
                {
                    Category newCategory = new Category();
                    // find the object...
                    lock (Categories)
                    {
                        foreach (Category category in Categories)
                        {
                            if (category.Name == CategoryName)
                            {
                                newCategory = category;
                                break;
                            }
                        }
                        // found it, now remove it...
                        Categories.Remove(newCategory);
                    }
                    // SavetheSettings
                    internal_http_server_object.Configuration.SaveSettings();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("AddCategory: " + e.Message);
                return false;
            }
        }

        public bool AddCategory(String CategoryName)
        {
            try
            {
                if (!CategoryExists(CategoryName))
                {
                    Category newCategory = new Category();

                    newCategory.Name = CategoryName;
                    newCategory.isAutomatic = true;

                    lock (Categories)
                    {
                        Categories.Add(newCategory);
                    }
                    // SavetheSettings
                    internal_http_server_object.Configuration.SaveSettings();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("AddCategory: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks if a category exists or not
        /// </summary>
        /// <param name="CategoryName"></param>
        /// <returns></returns>
        public bool CategoryExists(String CategoryName)
        {
            lock (Categories)
            {
                foreach(Category _category in Categories)
                {
                    if (CategoryName == _category.Name)
                        return true;
                }
            }
            return false;
        }
        #endregion
    }
}
