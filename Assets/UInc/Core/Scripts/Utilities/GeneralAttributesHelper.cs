using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Frontend.Scripts
{
	public static class GeneralAttributeHelper
	{
		public static TRet GetEnumAttributeValue<TObj, TAttr, TRet>(TObj obj, Func<TAttr, TRet> ret)
			where TObj : Enum
			where TAttr : Attribute
		{
			MemberInfo memberInfo = typeof(TObj).GetMember(obj.ToString()).FirstOrDefault();

			if (memberInfo != null)
			{
				TAttr attribute = (TAttr)memberInfo.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault();

				if (attribute != null)
				{
					return ret.Invoke(attribute);
				}
			}

			return default(TRet);
		}
	}
}
