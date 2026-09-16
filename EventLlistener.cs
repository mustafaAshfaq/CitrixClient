using System.Diagnostics.Tracing;

namespace WebApplication1.Storefront;

class NetEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("System.Net"))
            EnableEvents(eventSource, EventLevel.Verbose);
    }
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine(eventData.EventName + " - " + eventData.EventSource);
        if(eventData.Payload!=null && eventData.Payload.Count>0)
        {
            for(var i=0;i<eventData.Payload.Count;i++)
            Console.WriteLine("\t"+eventData.PayloadNames[i] + " " + eventData.Payload[i]+"\t");
        }
        Console.WriteLine(Environment.NewLine);
    }
}
