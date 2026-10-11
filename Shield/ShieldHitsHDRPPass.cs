using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace FXV
{
    public class ShieldHitsHDRPPass : CustomPass
    {
        CommandBuffer cmdBuff;

        internal CommandBuffer GetCommandBuffer()
        {
            return cmdBuff; 
        }

        public override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            cmdBuff = cmd;
        }

        public override void Execute(CustomPassContext ctx)
        {
            Shield._AddHitsToCommandBuffer(cmdBuff);
        }

        public override void Cleanup()
        {

        }
    }
}
