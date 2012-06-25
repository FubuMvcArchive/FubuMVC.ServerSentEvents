using System.Diagnostics;
using System.Text;
using FubuMVC.Core.Runtime;
using FubuCore;

namespace FubuMVC.ServerSentEvents
{
    public class ServerEventWriter : IServerEventWriter
    {
        public readonly string Data = "data: ";
        public readonly string Event = "event: ";
        public readonly string Id = "id: ";
        public readonly string Retry = "retry: ";

        private readonly IOutputWriter _writer;
        private bool _first = true;


        public ServerEventWriter(IOutputWriter writer)
        {
            _writer = writer;
        }

        public void WriteData(string data, string id = null, string @event = null, int? retry = null)
        {
            if (_first)
            {
                _writer.ContentType(MimeType.EventStream);
                _first = false;
            }

            var builder = new StringBuilder();
            
            if (@event.IsNotEmpty())
            {
                builder.Append(Id);
                builder.Append(id);
                builder.Append("/");
                builder.Append(@event);
                builder.Append("\n");
            }
            else
            {
                writeProp(builder, Id, id);
            }
            
            writeProp(builder, Retry, retry);
            writeProp(builder, Data, data);
            builder.Append("\n");

            _writer.Write(builder.ToString());

            // TEMPORARY
            Debug.WriteLine(builder.ToString());

            _writer.Flush();
        }

        public void Write(ServerEvent @event)
        {
            WriteData(@event.Data, @event.Id, @event.Event, @event.Retry);
        }

        private static void writeProp(StringBuilder builder, string flag, object text)
        {
            if (text == null) return;

            builder.Append(flag);
            builder.Append(text.ToString());
            builder.Append("\n");
        }
    }
}