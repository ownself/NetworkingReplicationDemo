using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// remote players are only existed on client, it only represent other players on client and has not authority, neither handling input
public class RemotePlayer : Player
{
	// add things only for remote players
	override protected string GetTypeName() { return "remote player:"; }
}