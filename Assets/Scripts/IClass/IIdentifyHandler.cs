using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIdentifyHandler
{
    public int Id { get; }

    public bool TryGetIdenfier(string id, out float value);
    public float GetIdentifier(string id);
    public void SetIdentifier(string id, float value);
}
