﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Tests
{
    public class TestEntityClasses
    {
        // this example shows how to use the C# class API for entities.

        public interface IChatRoom
        {
            DateTime Post(string content);

            List<KeyValuePair<DateTime, string>> Get();
        }

        public interface ICounter
        {
            void Increment();

            void Add(int value);

            Task<int> Get();

            void Set(int newValue);

            void Delete();
        }

        [FunctionName(nameof(ChatRoom))]
        public static Task ChatRoomFunction([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<ChatRoom>();
        }

        [FunctionName(nameof(CounterWithProxy))]
        public static Task CounterFunction([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CounterWithProxy>();
        }

        //-------------- an entity representing a chat room -----------------

        [JsonObject(MemberSerialization.OptIn)]
        public class ChatRoom : IChatRoom
        {
            public ChatRoom()
            {
                this.ChatEntries = new SortedDictionary<DateTime, string>();
            }

            [JsonProperty("chatEntries")]
            public SortedDictionary<DateTime, string> ChatEntries { get; set; }

            // an operation that adds a message to the chat
            public DateTime Post(string content)
            {
                var timestamp = DateTime.UtcNow;
                this.ChatEntries.Add(timestamp, content);
                return timestamp;
            }

            // an operation that reads all messages in the chat
            public List<KeyValuePair<DateTime, string>> Get()
            {
                return this.ChatEntries.ToList();
            }
        }

        //-------------- An entity representing a counter object -----------------

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class CounterWithProxy : ICounter
        {
            [JsonProperty("value")]
            public int Value { get; set; }

            public void Increment()
            {
                this.Value += 1;
            }

            public void Add(int value)
            {
                this.Value += value;
            }

            public Task<int> Get()
            {
                return Task.FromResult(this.Value);
            }

            public void Set(int newValue)
            {
                this.Value = newValue;
            }

            public void Delete()
            {
                Entity.Current.DestructOnExit();
            }
        }
    }
}