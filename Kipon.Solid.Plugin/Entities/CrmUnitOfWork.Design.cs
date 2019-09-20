// Version: 1.0.0, Dynamics 365 svcutil solid extension tool by Kipon ApS (c) 2019, Kjeld Poulsen
// This file is autogenerated. Do not touch the code manually.

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
namespace Kipon.Solid.Plugin.Entities
{
	[Kipon.Xrm.Attributes.Export(typeof(IUnitOfWork))]
	[Kipon.Xrm.Attributes.Export(typeof(Kipon.Xrm.IUnitOfWork))]
	public partial class CrmUnitOfWork: IUnitOfWork, IDisposable
	{
		private SolidContextService context;
		private IOrganizationService _service;
		public CrmUnitOfWork(IOrganizationService orgService)
		{
			this._service = orgService;
			this.context = new SolidContextService(_service);
		}

        public void Dispose()
        {
            context.Dispose();
        }

        public R ExecuteRequest<R>(OrganizationRequest request) where R : OrganizationResponse
        {
            return (R)this.context.Execute(request);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return this.context.Execute(request);
        }


        public Guid Create(Entity entity)
        {
            return this._service.Create(entity);
        }

        public void Update(Entity entity)
        {
            this._service.Update(entity);
        }

        public void Delete(Entity entity)
        {
            this._service.Delete(entity.LogicalName, entity.Id);
        }
        public void ClearChanges()
        {
            this.context.ClearChanges();
        }

        public void Detach(string logicalName, Guid? id)
        {
            if (this.context != null)
            {
                var candidates = (from c in this.context.GetAttachedEntities() where c.LogicalName == logicalName select c);
                if (id != null)
                {
                    candidates = (from c in candidates where c.Id == id.Value select c);
                }
                foreach (var r in candidates.ToArray())
                {
                    context.Detach(r);
                }
            }
        }

        public void SaveChanges() 
        {
            this.context.SaveChanges();
        }

		private Kipon.Xrm.IRepository<Account> _accounts; 
		public Kipon.Xrm.IRepository<Account> Accounts
		{
			get
			{
				if (_accounts == null)
					{
						_accounts = new CrmRepository<Account>(this.context);
					}
				return _accounts;
			}
		}
		private Kipon.Xrm.IRepository<Contact> _contacts; 
		public Kipon.Xrm.IRepository<Contact> Contacts
		{
			get
			{
				if (_contacts == null)
					{
						_contacts = new CrmRepository<Contact>(this.context);
					}
				return _contacts;
			}
		}
		private Kipon.Xrm.IRepository<Opportunity> _opportunities; 
		public Kipon.Xrm.IRepository<Opportunity> Opportunities
		{
			get
			{
				if (_opportunities == null)
					{
						_opportunities = new CrmRepository<Opportunity>(this.context);
					}
				return _opportunities;
			}
		}
		private Kipon.Xrm.IRepository<SalesOrder> _salesorders; 
		public Kipon.Xrm.IRepository<SalesOrder> Salesorders
		{
			get
			{
				if (_salesorders == null)
					{
						_salesorders = new CrmRepository<SalesOrder>(this.context);
					}
				return _salesorders;
			}
		}
		private Kipon.Xrm.IRepository<Quote> _quotes; 
		public Kipon.Xrm.IRepository<Quote> Quotes
		{
			get
			{
				if (_quotes == null)
					{
						_quotes = new CrmRepository<Quote>(this.context);
					}
				return _quotes;
			}
		}
		private Kipon.Xrm.IRepository<SystemUser> _systemusers; 
		public Kipon.Xrm.IRepository<SystemUser> Systemusers
		{
			get
			{
				if (_systemusers == null)
					{
						_systemusers = new CrmRepository<SystemUser>(this.context);
					}
				return _systemusers;
			}
		}
	}
	[Kipon.Xrm.Attributes.Export(typeof(IAdminUnitOfWork))]
	[Kipon.Xrm.Attributes.Export(typeof(Kipon.Xrm.IAdminUnitOfWork))]
	public partial class AdminCrmUnitOfWork : CrmUnitOfWork, Kipon.Xrm.IAdminUnitOfWork
	{
		public AdminCrmUnitOfWork(Microsoft.Xrm.Sdk.IOrganizationService org) : base(org) { }
	}
	public partial interface IAccountTarget : Kipon.Xrm.Target<Account>{ }
	public partial interface IAccountPreimage : Kipon.Xrm.Preimage<Account>{ }
	public partial interface IAccountPostimage : Kipon.Xrm.Postimage<Account>{ }
	public partial interface IAccountMergedimage : Kipon.Xrm.Mergedimage<Account>{ }
	public partial class Account :
		IAccountTarget,
		IAccountPreimage,
		IAccountPostimage,
		IAccountMergedimage
	{
	}
	public partial interface IContactTarget : Kipon.Xrm.Target<Contact>{ }
	public partial interface IContactPreimage : Kipon.Xrm.Preimage<Contact>{ }
	public partial interface IContactPostimage : Kipon.Xrm.Postimage<Contact>{ }
	public partial interface IContactMergedimage : Kipon.Xrm.Mergedimage<Contact>{ }
	public partial class Contact :
		IContactTarget,
		IContactPreimage,
		IContactPostimage,
		IContactMergedimage
	{
	}
	public partial interface IOpportunityTarget : Kipon.Xrm.Target<Opportunity>{ }
	public partial interface IOpportunityPreimage : Kipon.Xrm.Preimage<Opportunity>{ }
	public partial interface IOpportunityPostimage : Kipon.Xrm.Postimage<Opportunity>{ }
	public partial interface IOpportunityMergedimage : Kipon.Xrm.Mergedimage<Opportunity>{ }
	public partial class Opportunity :
		IOpportunityTarget,
		IOpportunityPreimage,
		IOpportunityPostimage,
		IOpportunityMergedimage
	{
	}
	public partial interface ISalesOrderTarget : Kipon.Xrm.Target<SalesOrder>{ }
	public partial interface ISalesOrderPreimage : Kipon.Xrm.Preimage<SalesOrder>{ }
	public partial interface ISalesOrderPostimage : Kipon.Xrm.Postimage<SalesOrder>{ }
	public partial interface ISalesOrderMergedimage : Kipon.Xrm.Mergedimage<SalesOrder>{ }
	public partial class SalesOrder :
		ISalesOrderTarget,
		ISalesOrderPreimage,
		ISalesOrderPostimage,
		ISalesOrderMergedimage
	{
	}
	public partial interface IQuoteTarget : Kipon.Xrm.Target<Quote>{ }
	public partial interface IQuotePreimage : Kipon.Xrm.Preimage<Quote>{ }
	public partial interface IQuotePostimage : Kipon.Xrm.Postimage<Quote>{ }
	public partial interface IQuoteMergedimage : Kipon.Xrm.Mergedimage<Quote>{ }
	public partial class Quote :
		IQuoteTarget,
		IQuotePreimage,
		IQuotePostimage,
		IQuoteMergedimage
	{
	}
	public partial interface ISystemUserTarget : Kipon.Xrm.Target<SystemUser>{ }
	public partial interface ISystemUserPreimage : Kipon.Xrm.Preimage<SystemUser>{ }
	public partial interface ISystemUserPostimage : Kipon.Xrm.Postimage<SystemUser>{ }
	public partial interface ISystemUserMergedimage : Kipon.Xrm.Mergedimage<SystemUser>{ }
	public partial class SystemUser :
		ISystemUserTarget,
		ISystemUserPreimage,
		ISystemUserPostimage,
		ISystemUserMergedimage
	{
	}
	public sealed class AccountReference : Kipon.Xrm.TargetReference<Account>
	{
		public AccountReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => Account.EntityLogicalName;
	}
	public sealed class ContactReference : Kipon.Xrm.TargetReference<Contact>
	{
		public ContactReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => Contact.EntityLogicalName;
	}
	public sealed class OpportunityReference : Kipon.Xrm.TargetReference<Opportunity>
	{
		public OpportunityReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => Opportunity.EntityLogicalName;
	}
	public sealed class SalesOrderReference : Kipon.Xrm.TargetReference<SalesOrder>
	{
		public SalesOrderReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => SalesOrder.EntityLogicalName;
	}
	public sealed class QuoteReference : Kipon.Xrm.TargetReference<Quote>
	{
		public QuoteReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => Quote.EntityLogicalName;
	}
	public sealed class SystemUserReference : Kipon.Xrm.TargetReference<SystemUser>
	{
		public SystemUserReference(EntityReference target): base(target){ }
		protected sealed override string _logicalName => SystemUser.EntityLogicalName;
	}
	public partial interface IUnitOfWork : Kipon.Xrm.IUnitOfWork
	{
		#region entity repositories
		Kipon.Xrm.IRepository<Account> Accounts { get; }
		Kipon.Xrm.IRepository<Contact> Contacts { get; }
		Kipon.Xrm.IRepository<Opportunity> Opportunities { get; }
		Kipon.Xrm.IRepository<SalesOrder> Salesorders { get; }
		Kipon.Xrm.IRepository<Quote> Quotes { get; }
		Kipon.Xrm.IRepository<SystemUser> Systemusers { get; }
		#endregion
	}
	public partial interface IAdminUnitOfWork : Kipon.Xrm.IAdminUnitOfWork, IUnitOfWork { }
   public class CrmRepository<T> : Kipon.Xrm.IRepository<T> where T: Microsoft.Xrm.Sdk.Entity, new() 
    {
        private SolidContextService context;

        public CrmRepository(SolidContextService context)
        {
            this.context = context;
        }

        public IQueryable<T> GetQuery()
        {
            return context.CreateQuery<T>();
        }

        public void Delete(T entity)
        {
            this.context.DeleteObject(entity);
        }

        public void Add(T entity)
        {
            this.context.AddObject(entity);
        }

        public void Attach(T entity)
        {
            this.context.Attach(entity);
        }

        public void Detach(T entity)
        {
            this.context.Detach(entity);
        }

        public void Update(T entity)
        {
            if (!this.context.IsAttached(entity))
            {
                this.context.Attach(entity);
            }

            this.context.UpdateObject(entity);
        }

        public T GetById(Guid id)
        {
            return (from q in this.GetQuery()
                    where q.Id == id
                    select q).Single();
        }
    }
}
