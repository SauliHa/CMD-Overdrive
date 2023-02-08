using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manticore
{
    public interface ISpecialTile
    {
        void OpenActionPrompt();
        void Action();
    }
}
