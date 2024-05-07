
using UnityEngine;

public class TicketDispenser : MonoBehaviour
{
    [SerializeField] protected Ticket ticketTemplate;
    [SerializeField, Range(10, 120)] protected float ticketsPerMinute;

    protected float rawTicketLength;
    protected float scaledTicketLength;

    protected EasyPool<Ticket> ticketPool;

    protected Ticket printingTicket;

    public int TicketsToPrint { get; protected set; }

    private void Awake()
    {
        ticketPool = new EasyPool<Ticket>(ticketTemplate, transform)
        {
            ReturnCriteria = (ticket) => !ticket.GetMeshRenderer.isVisible,
            ReturnAction = (ticket) =>
            {
                ticket.GetHingeJoint.connectedBody = null;
                ticket.gameObject.SetActive(false);
                ticket.transform.position = ticketTemplate.transform.position;
                ticket.transform.rotation = ticketTemplate.transform.rotation;
            },
        };
        scaledTicketLength = (rawTicketLength = ticketTemplate.GetMeshFilter.mesh.bounds.size.z) * ticketTemplate.transform.localScale.z;
    }

    public void Print(int amount) => TicketsToPrint += amount;

    public void Stop() => TicketsToPrint = 0;

    public void Clear()
    {
        Stop();
        ticketPool.ReturnAll();
    }

    private void Print()
    {
        Ticket previousTicket = printingTicket;
        printingTicket = ticketPool.Get();
        if (previousTicket)
        {
            SetHingeJoint(previousTicket.GetHingeJoint, printingTicket.GetRigidbody);
            previousTicket.GetRigidbody.isKinematic = false;
        }
        printingTicket.GetRigidbody.isKinematic = true;
    }

    protected void SetHingeJoint(HingeJoint hingeJoint, Rigidbody connectedBody)
    {
        hingeJoint.connectedBody = connectedBody;
        hingeJoint.anchor = Vector3.forward * rawTicketLength;
        hingeJoint.connectedAnchor = Vector3.zero;
    }

    protected bool ContinueMovingPrintingTicket() => printingTicket && Mathf.Abs(printingTicket.transform.position.z - ticketTemplate.transform.position.z) < scaledTicketLength;

    private void FixedUpdate()
    {
        if (TicketsToPrint > 0 && !ContinueMovingPrintingTicket())
        {
            TicketsToPrint--;
            Print();
        }

        if (ContinueMovingPrintingTicket())
            printingTicket.transform.position += Vector3.back * scaledTicketLength * ticketsPerMinute * Time.deltaTime / 60f;
    }
}
