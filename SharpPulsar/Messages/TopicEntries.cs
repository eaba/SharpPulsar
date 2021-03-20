﻿namespace SharpPulsar.Messages
{
    internal sealed class TopicEntries
    {
        public TopicEntries(long? entries)
        {
            Entries = entries;
        }

        public long? Entries { get; }
    }
}
