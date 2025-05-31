using UnityEngine;
using Unity.Netcode.Components;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // Return false here to allow the owner client to write transform data.
    protected override bool OnIsServerAuthoritative() => false;
}
