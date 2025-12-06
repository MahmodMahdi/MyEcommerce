using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IUnitOfWork:IDisposable
	{
		ICategoryRepository CategoryRepository { get; }
		int complete();
	}
}
