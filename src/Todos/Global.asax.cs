﻿using System;
using ServiceStack;
using ServiceStack.Redis;
using Funq;

//The entire C# source code for the ServiceStack + Redis TODO REST backend. There is no other .cs :)
namespace Todos
{
    // Define your ServiceStack web service request (i.e. Request DTO).
    [Route("/todos")]
    [Route("/todos/{Id}")]
    public class Todo
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public bool Done { get; set; }
    }

    // Create your ServiceStack rest-ful web service implementation. 
    public class TodoService : Service
    {
        public object Get(Todo todo)
        {
            //Return a single Todo if the id is provided.
            if (todo.Id != default)
                return Redis.As<Todo>().GetById(todo.Id);

            //Return all Todos items.
            return Redis.As<Todo>().GetAll();
        }

        // Handles creating and updating the Todo items.
        public Todo Post(Todo todo)
        {
            var redis = Redis.As<Todo>();
            
            //Get next id for new todo
            if (todo.Id == default) 
                todo.Id = redis.GetNextSequence();
            
            redis.Store(todo);
            
            return todo;
        }

        // Handles creating and updating the Todo items.
        public Todo Put(Todo todo) => Post(todo);

        // Handles Deleting the Todo item
        public void Delete(Todo todo) => Redis.As<Todo>().DeleteById(todo.Id);
    }

    // Create your ServiceStack web service application with a singleton AppHost.
    public class AppHost : AppHostBase
    {
        // Initializes your ServiceStack App Instance, with the specified assembly containing the services.
        public AppHost() : base("Backbone.js TODO", typeof(TodoService).Assembly) { }

        // Configure the container with the necessary routes for your ServiceStack application.
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                UseCamelCase = true
            });

            //Register Redis factory in Funq IoC. The default port for Redis is 6379.
            container.Register<IRedisClientsManager>(new BasicRedisClientManager("localhost:6379"));
        }
    }

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            //Initialize your ServiceStack AppHost
            new AppHost().Init();
        }
    }
}
