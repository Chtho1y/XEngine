#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;


namespace XLua
{
    public partial class DelegateBridge : DelegateBridgeBase
    {
		
		public void __Gen_Delegate_Imp0()
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                RealStatePtr L = luaEnv.rawL;
                int errFunc = LuaAPI.pcall_prepare(L, errorFuncRef, luaReference);
                
                
                PCall(L, 0, 0, errFunc);
                
                
                
                LuaAPI.lua_settop(L, errFunc - 1);
                
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		public void __Gen_Delegate_Imp1(string p0)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                RealStatePtr L = luaEnv.rawL;
                int errFunc = LuaAPI.pcall_prepare(L, errorFuncRef, luaReference);
                
                LuaAPI.lua_pushstring(L, p0);
                
                PCall(L, 1, 0, errFunc);
                
                
                
                LuaAPI.lua_settop(L, errFunc - 1);
                
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		public void __Gen_Delegate_Imp2(bool p0)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                RealStatePtr L = luaEnv.rawL;
                int errFunc = LuaAPI.pcall_prepare(L, errorFuncRef, luaReference);
                
                LuaAPI.lua_pushboolean(L, p0);
                
                PCall(L, 1, 0, errFunc);
                
                
                
                LuaAPI.lua_settop(L, errFunc - 1);
                
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
        
		static DelegateBridge()
		{
		    Gen_Flag = true;
		}
		
		public override Delegate GetDelegateByType(Type type)
		{
		
		    if (type == typeof(XEngine.Engine.LuaOnGameEnter))
			{
			    return new XEngine.Engine.LuaOnGameEnter(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnCheckUpdateFinished))
			{
			    return new XEngine.Engine.LuaOnCheckUpdateFinished(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnUpdate))
			{
			    return new XEngine.Engine.LuaOnUpdate(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnFixedUpdate))
			{
			    return new XEngine.Engine.LuaOnFixedUpdate(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnLateUpdate))
			{
			    return new XEngine.Engine.LuaOnLateUpdate(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnLowMemory))
			{
			    return new XEngine.Engine.LuaOnLowMemory(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnApplicationQuit))
			{
			    return new XEngine.Engine.LuaOnApplicationQuit(__Gen_Delegate_Imp0);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnSceneChanged))
			{
			    return new XEngine.Engine.LuaOnSceneChanged(__Gen_Delegate_Imp1);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnException))
			{
			    return new XEngine.Engine.LuaOnException(__Gen_Delegate_Imp1);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnApplicationFocus))
			{
			    return new XEngine.Engine.LuaOnApplicationFocus(__Gen_Delegate_Imp2);
			}
		
		    if (type == typeof(XEngine.Engine.LuaOnApplicationPause))
			{
			    return new XEngine.Engine.LuaOnApplicationPause(__Gen_Delegate_Imp2);
			}
		
		    return null;
		}
	}
    
}