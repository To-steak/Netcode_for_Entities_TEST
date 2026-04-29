using UnityEngine;
using Unity.NetCode;

public class AutoconnectBootstrap : ClientServerBootstrap
{
    /// <summary>
    /// Server World와 Client World를 더 이상 생성하지 않음 
    /// </summary>
    /// <param name="defaultWorldName"></param>
    /// <returns></returns>
    public override bool Initialize(string defaultWorldName)
    {
        return false;
    }
}
