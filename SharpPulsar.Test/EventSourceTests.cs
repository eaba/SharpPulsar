﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Samples;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Configuration;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Handlers;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test
{/// <summary>
/// Pulsar LAC may affect test success. Replay tests
/// </summary>
    public class EventSourceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly PulsarSystem _pulsarSystem;
        private string _topic;
        private int _amount;
        private IActorRef _producer;
        public EventSourceTests(ITestOutputHelper output)
        {
            _topic = Guid.NewGuid().ToString();
            _output = output;
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .UseProxy(false)
                .OperationTimeout(30000)
                .Authentication(new AuthenticationDisabled())
                //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

            _pulsarSystem = PulsarSystem.GetInstance(clientConfig);
            _amount = 100;
            ProduceMessages();
        }
        [Fact]
        private void Get_Number_Of_Entries()
        {
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            _output.WriteLine($"TopicEntries: {JsonSerializer.Serialize(numb, new JsonSerializerOptions { WriteIndented = true })}");
            Assert.True(numb.Max > 80);
            
            var num = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            _output.WriteLine($"NumOfEntries: {JsonSerializer.Serialize(num, new JsonSerializerOptions { WriteIndented = true })}");
            Assert.True(numb.Max > 80);
        }
        [Fact]
        private  void Replay_Topic_Custom_Handler()
        {
            _amount = 100;
            var replayed = 0;
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(_topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 0, 99, numb.Max.Value, null, false);
            foreach (var msg in _pulsarSystem.EventSource(replay, message =>
            {
                if (message is EventMessage evt)
                {
                    var m = evt.Message.ToTypeOf<Students>();
                    _output.WriteLine($"Sequence Id: {evt.SequenceId}");
                    return m;
                }

                var t =  message as NotTagged;
                _output.WriteLine($"Sequence Id: {t?.SequenceId}");
                return t?.Message.ToTypeOf<Students>();
            }))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(replayed > 95);
        }
        [Fact]
        private  void Replay_Topic()
        {
            _amount = 10;
            var replayed = 0;
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(_topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 1, 6, numb.Max.Value, null, false);
            foreach (var msg in _pulsarSystem.EventSource(replay, e =>
            {

                if (e is EventMessage evt)
                {
                    var m = evt.Message.ToTypeOf<Students>();
                    _output.WriteLine($"Sequence Id: {evt.SequenceId}");
                    return m;
                }

                var t = e as NotTagged;
                _output.WriteLine($"Sequence Id: {t?.SequenceId}");
                return t?.Message.ToTypeOf<Students>();

            }))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(replayed > 4);
        }
        [Fact]
        private  void Replay_Topic_To_Greater()
        {
            _amount = 100;
            var replayed = 0;
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(_topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 0, 101, numb.Max.Value, null, false);
            foreach (var msg in _pulsarSystem.EventSource<Students>(replay))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(replayed > 95 && replayed < 101);
        }
        [Fact]
        private  void Next_Tagged_Topic()
        {
            _amount = 100;
            var replayed = 0; 
            var topic = $"persistent://public/default/journal-event*";
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(JournalEntry));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 0, 15, 50, new Tag("Tag", "utc"), true);
            foreach (var msg in _pulsarSystem.EventSource<JournalEntry>(replay))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            //SharpPulsar deducts 2 from the max.
            Assert.True(replayed > 1);
            replayed = 0;
            var num = _pulsarSystem.EventSource(new GetNumberOfEntries(topic, "http://localhost:8080"));
            foreach (var msg in _pulsarSystem.EventSource<JournalEntry>(new NextPlay(topic, 50, 16, num.TotalEntries.Value, true)))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(replayed > 1);
        }
        [Fact]
        private  void Next_Topic()
        {
            _amount = 100;
            var replayed = 0;
            _topic = $"persistent://public/default/{Guid.NewGuid()}";
            ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(_topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 1, 49, numb.Max.Value, null, false);
            foreach (var msg in _pulsarSystem.EventSource<Students>(replay))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            //SharpPulsar deducts 2 from the max.
            Assert.True(replayed > 45);
            replayed = 0;
            var num = _pulsarSystem.EventSource(new GetNumberOfEntries(_topic, "http://localhost:8080"));
            foreach (var msg in _pulsarSystem.EventSource<Students>(new NextPlay(_topic, num.Max.Value, 51, 99)))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.True(replayed > 45);
        }
        [Fact]
        private void Replay_Tagged_Topic_To_Greater()
        {
            _amount = 100;
            var replayed = 0;
            var topic = $"persistent://public/default/journal-event*";
            //ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(JournalEntry));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 0, 50, 35, new Tag("Tag", "utc"), true);
            foreach (var msg in _pulsarSystem.EventSource<JournalEntry>(replay))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            _output.WriteLine($"replayed:{replayed}");
            Assert.True(replayed > 6);
        }
        [Fact]
        private void Replay_Tagged_Topic()
        {
            _amount = 100;
            var replayed = 0;
            var topic = $"persistent://public/default/journal-event*";
            //ProduceMessages();
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var readerListener = new DefaultMessageListener(null, null);
            var jsonSchem = AvroSchema.Of(typeof(JournalEntry));
            var readerConfig = new ReaderConfigBuilder()
                .ReaderName("event-reader")
                .Schema(jsonSchem)
                .EventListener(consumerListener)
                .ReaderListener(readerListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
            var numb = _pulsarSystem.EventSource(new GetNumberOfEntries(topic, "http://localhost:8080"));
            var replay = new ReplayTopic(readerConfig, "http://localhost:8080", 0, 35, 50, new Tag("Tag", "utc"), true);
            foreach (var msg in _pulsarSystem.EventSource<JournalEntry>(replay))
            {
                replayed++;
                _output.WriteLine(JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true }));
            }
            _output.WriteLine($"replayed: {replayed}");
            Assert.True(replayed >= 6);
        }

        private void ProduceMessages()
        {
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName(_topic)
                .Topic(_topic)
                .Schema(jsonSchem)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = _producer ?? _pulsarSystem.PulsarProducer(new CreateProducer(jsonSchem, producerConfig)).Producer;

            var sends = new List<Send>();
            for (var i = 1L; i <= _amount; i++)
            {
                var student = new Students
                {
                    Name = $"#LockDown Ebere: {DateTimeOffset.Now.ToUnixTimeMilliseconds()} - test {DateTime.Now.ToString(CultureInfo.InvariantCulture)}",
                    Age = 2020 + (int)i,
                    School = "Akka-Pulsar university"
                };
                var metadata = new Dictionary<string, object>
                {
                    ["SequenceId"] = i,
                    ["Key"] = "Bulk",
                    ["Properties"] = new Dictionary<string, string>
                    {
                        { "Tick", DateTime.Now.Ticks.ToString() },
                        {"Week-Day", "Saturday" }
                    }
                };
                sends.Add(new Send(student, metadata.ToImmutableDictionary(), $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"));
            }
            var bulk = new BulkSend(sends, _topic);
            _pulsarSystem.BulkSend(bulk, t);
        }
    }
}
