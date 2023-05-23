using System;
using HutongGames.PlayMaker;

namespace ShowBoltSize
{
    // Token: 0x02000002 RID: 2
    [ActionCategory( (ActionCategory) 11 )]
	internal class FsmHook : FsmStateAction
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override void OnEnter()
		{
            Call?.Invoke();
            Finish();
		}

		// Token: 0x04000001 RID: 1
		public Action Call;
	}
}
