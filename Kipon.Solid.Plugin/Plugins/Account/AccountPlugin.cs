﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kipon.Xrm.Attributes;

namespace Kipon.Solid.Plugin.Plugins.Account
{
    public class AccountPlugin
    {
        public void OnPreDelete(Entities.AccountReference accountRef)
        {
        }

        public void OnPreCreate(Entities.Account.IAccountNameChanged target, ServiceAPI.IAccountService accountService)
        {
            accountService.OnNameChanged(target);
        }

        [Sort(100)]
        public void OnPreUpdate(Entities.Account.IAccountNameChanged target, ServiceAPI.IAccountService accountService)
        {
            accountService.OnNameChanged(target);
        }

        [Sort(101)]
        public void OnPreUpdate(Entities.Account.ICreditLimitChanged target)
        {
            // do something when changes
        }
    }
}
