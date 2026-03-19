using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGAME
{
    [Serializable]
    public class RoomRuntimeNode : RuntimeNode
    {
        public string SceneGuid;

        public List<RegionConnectionData> ConnectedEntrances;
        public List<RegionConnectionData> ConnectedExits;

        public List<string> EntranceSourceSceneGuids;
        public List<string> ExitDestinationSceneGuids;
    }
}
/*
 * Notes
 * 
 * So runtime nodes need to be just data not editor code.
 * 
 * 
 * Problem:
 * right now graph is only saving connections based on port name and node id.
 * can get correct scene from node id. but port names are prob not currently unique enough. 
 *  
 *  Solution:
 *  refactor graph to stor ports with unique (to the scen they are in) ids for look up and 
 *  allow non unique display text
 *  
 *  I already have the input and output ports as lists managed by my code, should be able to pair them with their data. 
 *  
 *  
 *  QUESTION: does this system merrit implementing a serializable dictionary datastructure for unity?
 *  
 *  a lot of these look ups I would do with dictionaries in runtime code, but because unity won't serialize those I am using synchronized lists....
 *  
 *  TODO: look up refresher on what datastruct is under the hood for unity's List<> class, to make sure I'm even using the right collection type
 */