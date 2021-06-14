using System;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// Walks through all the objects in a graph.
/// Taken from MSDN DotNet API Here:
/// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.objectmanager?view=net-5.0
/// </summary>
public sealed class ObjectWalker : IEnumerable, IEnumerator
{
    private Object m_current;

    // This stack contains the set of objects that will be enumerated.
    private Stack m_toWalk = new Stack();

    // The ObjectIDGenerator ensures that each object is enumerated just once.
    private ObjectIDGenerator m_idGen = new ObjectIDGenerator();

    // Construct an ObjectWalker passing the root of the object graph.
    public ObjectWalker(Object root)
    {
        Schedule(root);
    }

    // Return an enumerator so this class can be used with foreach.
    public IEnumerator GetEnumerator()
    {
        return this;
    }

    // Resetting the enumerator is not supported.
    public void Reset()
    {
        throw new NotSupportedException("Resetting the enumerator is not supported.");
    }

    // Return the enumeration's current object.
    public Object Current { get { return m_current; } }

    // Walk the reference of the passed-in object.
    private void Schedule(Object toSchedule)
    {
        if (toSchedule == null) return;

        // Ask the ObjectIDManager if this object has been examined before.
        Boolean firstOccurrence;
        m_idGen.GetId(toSchedule, out firstOccurrence);

        // If this object has been examined before, do not look at it again just return.
        if (!firstOccurrence) return;

        if (toSchedule.GetType().IsArray)
        {
            // The object is an array, schedule each element of the array to be looked at.
            foreach (Object item in ((Array)toSchedule)) Schedule(item);
        }
        else
        {
            // The object is not an array, schedule this object to be looked at.
            m_toWalk.Push(toSchedule);
        }
    }

    // Advance to the next item in the enumeration.
    public Boolean MoveNext()
    {
        // If there are no more items to enumerate, return false.
        if (m_toWalk.Count == 0) return false;

        // Check if the object is a terminal object (has no fields that refer to other objects).
        if (!IsTerminalObject(m_current = m_toWalk.Pop()))
        {
            // The object does have field, schedule the object's instance fields to be enumerated.
            foreach (FieldInfo fi in m_current.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Schedule(fi.GetValue(m_current));
            }
        }
        return true;
    }

    // Returns true if the object has no data fields with information of interest.
    private Boolean IsTerminalObject(Object data)
    {
        Type t = data.GetType();
        return t.IsPrimitive || t.IsEnum || t.IsPointer || data is String;
    }
}