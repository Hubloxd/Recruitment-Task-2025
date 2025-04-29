using System.Text.Json.Serialization;
using Recruitment_Task_2025.Data.Models;

namespace Recruitment_Task_2025
{
    /// Class to represent HATEOAS links.
    public class Link(string href, string rel, string method)
    {
        public String Href { get; set; } = href;
        public String Rel { get; set; } = rel;
        public String Method { get; set; } = method;
    }

    public class TodoItemResponse
    {
        public TodoItem Item { get; set; }
        [JsonPropertyName("_links")] public List<Link> Links { get; set; } = [];

        public TodoItemResponse(TodoItem todoItem)
        {
            Item = todoItem;
            // Add self link
            Links.Add(new Link($"/todos/{Item.Id}", "self", "GET"));
            // Add update link
            Links.Add(new Link($"/todos/{Item.Id}", "update", "PATCH"));
            // Add delete link
            Links.Add(new Link($"/todos/{Item.Id}", "delete", "DELETE"));
            // Add complete link
            Links.Add(new Link($"/todos/{Item.Id}/complete", "complete", "POST"));
            // Add set-completion link
            Links.Add(new Link($"/todos/{Item.Id}/completion", "set-completion", "PATCH"));
        }

        [JsonConstructor]
        public TodoItemResponse()
        {
            Item = new TodoItem() { Title = ""};
        }
    }

    public class TodoItemCollectionResponse
    {
        [JsonPropertyName("results")] public List<TodoItem> Items { get; set; }
        [JsonPropertyName("_links")] public List<Link> Links { get; set; } = [];

        public TodoItemCollectionResponse(List<TodoItem> todoItems)
        {
            Items = todoItems;

            // Add create link
            Links.Add(new Link("/todos", "create", "POST"));

            // Add timeframe links
            Links.Add(new Link("/todos/today", "next", "GET"));
            Links.Add(new Link("/todos/tomorrow", "next", "GET"));
            Links.Add(new Link("/todos/thisweek", "next", "GET"));
        }

        [JsonConstructor]
        public TodoItemCollectionResponse()
        {
            Items = new List<TodoItem>();
        }
    }
}