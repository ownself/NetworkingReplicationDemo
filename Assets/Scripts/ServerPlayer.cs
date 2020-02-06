using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Server players are only existed on server, in charge of movement and logic update, it has authority
public class ServerPlayer : Player
{
	// add things only for server players
	override protected string GetTypeName() { return "server player:"; }
}