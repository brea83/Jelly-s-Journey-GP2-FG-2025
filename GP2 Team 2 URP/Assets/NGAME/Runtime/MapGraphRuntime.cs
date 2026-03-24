using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NGAME
{
    public class MapGraphRuntime : MonoBehaviour
    {
        [SerializeField]
        protected RoomGraph m_Graph;

        private void Start()
        {
            StringBuilder sb = new StringBuilder();

            m_Graph.PrintGraph();

        }
    }
}
