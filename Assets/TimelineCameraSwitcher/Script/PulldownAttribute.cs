using UnityEngine;
public class PulldownAttribute : PropertyAttribute
{
    public string[] names;
    public PulldownAttribute(params string[] names)
    {
        this.names = names;
    }
}