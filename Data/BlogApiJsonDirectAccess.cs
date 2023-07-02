using Data.Models.Interfaces;
using Data.Models.Models;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Xml.Linq;

namespace Data
{
    public class BlogApiJsonDirectAccess : IBlogApi
    {
        BlogApiJsonDirectAccessSetting _settings;
        private List<BlogPost>? _blogPosts;
        private List<Category>? _categories;
        private List<Tag>? _tags;

        // We use IOptions to be able to read settings
        public BlogApiJsonDirectAccess(IOptions<BlogApiJsonDirectAccessSetting> options)
        {
            _settings = options.Value;
            if (!Directory.Exists(_settings.DataPath))
            {
                Directory.CreateDirectory(_settings.DataPath);
            }
            if (!Directory.Exists($@"{_settings.DataPath}\{_settings.BlogPostsFolder}"))
            {
                Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.BlogPostsFolder}");
            }
            if (!Directory.Exists($@"{_settings.DataPath}\{_settings.CategoriesFolder}"))
            {
                Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.CategoriesFolder}");
            }
            if (!Directory.Exists($@"{_settings.DataPath}\{_settings.TagsFolder}"))
            {
                Directory.CreateDirectory($@"{_settings.DataPath}\{_settings.TagsFolder}");
            }
        }

        // Loading data from our filesystem 
        private void Load<T>(ref List<T>? list, string folder)
        {
            if (list == null)
            {
                list = new();
                var fullpath = $@"{_settings.DataPath}\{folder}";
                foreach (var f in Directory.GetFiles(fullpath))
                {
                    var json = File.ReadAllText(f);
                    var bp = JsonSerializer.Deserialize<T>(json);
                    if (bp != null)
                    {
                        list.Add(bp);
                    }
                }
            }
        }
        private Task LoadBlogPostsAsync()
        {
            Load<BlogPost>(ref _blogPosts, _settings.BlogPostsFolder);
            return Task.CompletedTask;
        }
        private Task LoadTagsAsync()
        {
            Load<Tag>(ref _tags, _settings.TagsFolder);
            return Task.CompletedTask;
        }
        private Task LoadCategoriesAsync()
        {
            Load<Category>(ref _categories, _settings.CategoriesFolder);
            return Task.CompletedTask;
        }

        // Add a couple of methods to help manipulate the data
        private async Task SaveAsync<T>(List<T>? list, string folder, string filename, T item)
        {
            var filepath = $@"{_settings.DataPath}\{folder}\{filename}";
            await File.WriteAllTextAsync(filepath, JsonSerializer.
           Serialize<T>(item));
            if (list == null)
            {
                list = new();
            }
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
        private void DeleteAsync<T>(List<T>? list, string folder, string id)
        {
            var filepath = $@"{_settings.DataPath}\{folder}\{id}.json";
            try
            {
                File.Delete(filepath);
            }
            catch { }
        }

        // Implements the API by adding the methods to get blog posts
        public async Task<List<BlogPost>?> GetBlogPostsAsync(int numberofposts, int startindex)
        {
            await LoadBlogPostsAsync();
            return _blogPosts ?? new();
        }
        public async Task<BlogPost?> GetBlogPostAsync(string id)
        {
            await LoadBlogPostsAsync();
            if (_blogPosts == null)
                throw new Exception("Blog posts not found");
            return _blogPosts.FirstOrDefault(b => b.Id == id);
        }
        public async Task<int> GetBlogPostCountAsync()
        {
            await LoadBlogPostsAsync();
            if (_blogPosts == null)
                return 0;
            else
                return _blogPosts.Count();
        }

        // returns the current blog post count
        public async Task<List<Category>?> GetCategoriesAsync()
        {
            await LoadCategoriesAsync();
            return _categories ?? new();
        }
        public async Task<Category?> GetCategoryAsync(string id)
        {
            await LoadCategoriesAsync();
            if (_categories == null)
                throw new Exception("Categories not found");
            return _categories.FirstOrDefault(b => b.Id == id);
        }

        public async Task<List<Tag>?> GetTagsAsync()
        {
            await LoadTagsAsync();
            return _tags ?? new();
        }
        public async Task<Tag?> GetTagAsync(string id)
        {
            await LoadTagsAsync();
            if (_tags == null)
                throw new Exception("Tags not found");
            return _tags.FirstOrDefault(b => b.Id == id);
        }

        // Methods for saving, blog posts, categories, and tags.
        public async Task<BlogPost?> SaveBlogPostAsync(BlogPost item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
            }
            await SaveAsync<BlogPost>(_blogPosts, _settings.BlogPostsFolder, $"{item.Id}.json", item);
            return item;
        }

        public async Task<Category?> SaveCategoryAsync(Category item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
            }
            await SaveAsync<Category>(_categories, _settings.
           CategoriesFolder, $"{item.Id}.json", item);
            return item;
        }
        public async Task<Tag?> SaveTagAsync(Tag item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
            }
            await SaveAsync<Tag>(_tags, _settings.TagsFolder, $"{item.Id}.json", item);
         return item;
        }

        // Methods for deleting the items that we have created.

        /// <summary>
        /// Deletes a blog post with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the blog post to delete.</param>
        /// <returns>A task representing the asynchronous deletion operation.</returns>
        public Task DeleteBlogPostAsync(string id)
        {
            DeleteAsync(_blogPosts, _settings.BlogPostsFolder, id);
            if (_blogPosts != null)
            {
                var item = _blogPosts.FirstOrDefault(b => b.Id == id);
                if (item != null)
                {
                    _blogPosts.Remove(item);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a category with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>A task representing the asynchronous deletion operation.</returns>
        public Task DeleteCategoryAsync(string id)
        {
            DeleteAsync(_categories, _settings.CategoriesFolder, id);
            if (_categories != null)
            {
                var item = _categories.FirstOrDefault(b => b.Id == id);
                if (item != null)
                {
                    _categories.Remove(item);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a tag with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the tag to delete.</param>
        /// <returns>A task representing the asynchronous deletion operation.</returns>
        public Task DeleteTagAsync(string id)
        {
            DeleteAsync(_tags, _settings.TagsFolder, id);
            if (_tags != null)
            {
                var item = _tags.FirstOrDefault(b => b.Id == id);
                if (item != null)
                {
                    _tags.Remove(item);
                }
            }
            return Task.CompletedTask;
        }

        // Method for clearing the cache.

        /// <summary>
        /// Invalidates the cache by resetting the cached data.
        /// </summary>
        /// <returns>A task representing the asynchronous cache invalidation operation.</returns>
        public Task InvalidateCacheAsync()
        {
            _blogPosts = null;
            _tags = null;
            _categories = null;
            return Task.CompletedTask;
        }
    }
}
            //Explanation:

            //- The help comments provide clear and concise summaries of what each method does and the purpose of the parameters and return values.
            //- The comments mention that these methods are used for deleting specific items(blog posts, categories, and tags) that have been created.
            //- Each method takes an ID parameter to identify the specific item to be deleted.
            //- The comments state that these methods perform asynchronous deletion operations and return a `Task` to represent the completion of the operation.
            //- The comment for the `InvalidateCacheAsync` method indicates that it clears the cache by resetting the cached data for blog posts, tags, and categories.
            //- The comment also specifies that the cache clearing operation is performed asynchronously and returns a `Task` to represent completion.

            //These help comments provide useful documentation for future developers who may need to understand and work with this code, making it easier for them to grasp the purpose and usage of each method.
