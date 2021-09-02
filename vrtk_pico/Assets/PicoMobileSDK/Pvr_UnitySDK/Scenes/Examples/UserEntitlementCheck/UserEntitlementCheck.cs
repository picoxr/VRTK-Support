using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEntitlementCheck : MonoBehaviour {

	private void OnEnable()
	{
		Pvr_UnitySDKManager.EntitlementCheckResultEvent += HandelEntitlementCheckResult;
	}

	private void OnDisable()
	{
		Pvr_UnitySDKManager.EntitlementCheckResultEvent -= HandelEntitlementCheckResult;
	}

	void HandelEntitlementCheckResult(int resultCode)
	{
		Debug.Log("The User Entitlement Check Result is :" + resultCode);
		switch (resultCode)
		{
			case 0:
				Debug.Log("The User Entitlement Check Result is: success");
				// Todo
				
				break;
			case -1:
				Debug.Log("The User Entitlement Check Result is: invalid params");
				// Todo

				break;
			case -2:
				Debug.Log("The User Entitlement Check Result is: service not exist");
				// Todo

				break;
			case -3:
				Debug.Log("The User Entitlement Check Result is: bind failed");
				// Todo

				break;
			case -4:
				Debug.Log("The User Entitlement Check Result is: exception");
				// Todo

				break;
			case -5:
				Debug.Log("The User Entitlement Check Result is: timeout");
				// Todo

				break;
			case 10:
				Debug.Log("The User Entitlement Check Result is: missing package name");
				// Todo

				break;
			case 11:
				Debug.Log("The User Entitlement Check Result is: missing appid");
				// Todo

				break;
			case 13:
				Debug.Log("The User Entitlement Check Result is: appid and package name not match");
				// Todo

				break;
			case 20:
				Debug.Log("The User Entitlement Check Result is: not login");
				// Todo

				break;
			case 21:
				Debug.Log("The User Entitlement Check Result is: not pay");
				// Todo

				break;
			case 31:
				Debug.Log("The User Entitlement Check Result is: invalid sn");
				// Todo

				break;
			default:
				Debug.Log("The User Entitlement Check Result is: unknown");
				// Todo
				break;

		}
	}
}
