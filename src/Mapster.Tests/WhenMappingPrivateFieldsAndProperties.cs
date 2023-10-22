﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingPrivateFieldsAndProperties
    {
        [TestInitialize]
        public void Setup()
        {
            TypeAdapterConfig.GlobalSettings.Clear();
        }

        [TestMethod]
        public void Default_Settings_Should_Not_Map_Private_Fields_To_New_Object()
        {
            TypeAdapterConfig<CustomerWithPrivateField, CustomerDTO>
                .NewConfig()
                .NameMatchingStrategy(NameMatchingStrategy.Flexible);

            var customerId = 1;
            var customerName = "Customer 1";
            var aCustomer = new CustomerWithPrivateField(customerId, customerName);

            var dto = aCustomer.Adapt<CustomerDTO>();

            Assert.IsNotNull(dto);
            dto.Id.ShouldNotBe(customerId);
            dto.Name.ShouldBe(customerName);
        }

        [TestMethod]
        public void Default_Settings_Should_Not_Map_Private_Properties_To_New_Object()
        {
            var customerId = 1;
            var customerName = "Customer 1";
            var aCustomer = new CustomerWithPrivateProperty(customerId, customerName);

            var dto = aCustomer.Adapt<CustomerDTO>();

            Assert.IsNotNull(dto);
            dto.Id.ShouldBe(customerId);
            dto.Name.ShouldNotBe(customerName);
        }

        [TestMethod]
        public void Should_Map_Private_Field_To_New_Object_Correctly()
        {
            SetUpMappingNonPublicFields<CustomerWithPrivateField, CustomerDTO>();

            var customerId = 1;
            var customerName = "Customer 1";
            var aCustomer = new CustomerWithPrivateField(customerId, customerName);

            var dto = aCustomer.Adapt<CustomerDTO>();

            Assert.IsNotNull(dto);
            dto.Id.ShouldBe(customerId);
            dto.Name.ShouldBe(customerName);
        }

       
        [TestMethod]
        public void Should_Map_Private_Property_To_New_Object_Correctly()
        {
            SetUpMappingNonPublicProperties<CustomerWithPrivateProperty, CustomerDTO>();

            var customerId = 1;
            var customerName = "Customer 1";
            var aCustomer = new CustomerWithPrivateProperty(customerId, customerName);

            var dto = aCustomer.Adapt<CustomerDTO>();

            Assert.IsNotNull(dto);
            dto.Id.ShouldBe(customerId);
            dto.Name.ShouldBe(customerName);
        }

        [Ignore]
        [TestMethod]
        public void Should_Map_To_Private_Fields_Correctly() // this test not testing this scenario 
        {
            SetUpMappingNonPublicFields<CustomerDTO, CustomerWithPrivateField>();
            
            var dto = new CustomerDTO
            {
                Id = 1,
                Name = "Customer 1"
            };

            var customer = dto.Adapt<CustomerWithPrivateField>(); // creation as Record when constructor private member not used

            Assert.IsNotNull(customer);
            Assert.IsTrue(customer.HasId(dto.Id));
            customer.Name.ShouldBe(dto.Name);            
        }

        [Ignore]
        [TestMethod]
        public void Should_Map_To_Private_Properties_Correctly() // this test not testing this scenario 
        {
            SetUpMappingNonPublicFields<CustomerDTO, CustomerWithPrivateProperty>();

            var dto = new CustomerDTO
            {
                Id = 1,
                Name = "Customer 1"
            };

            var customer = dto.Adapt<CustomerWithPrivateProperty>(); // creation  as Record when constructor private member not used

            Assert.IsNotNull(customer);
            customer.Id.ShouldBe(dto.Id);
            Assert.IsTrue(customer.HasName(dto.Name));
        }

        [TestMethod]
        public void Should_Map_To_Private_Properties_Using_Include()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<CustomerWithProtectedProperty, CustomerDTO>()
                .IncludeMember((model, side) => model.AccessModifier == AccessModifier.Protected);

            var customerId = 1;
            var customerName = "Customer 1";
            var aCustomer = new CustomerWithProtectedProperty(customerId, customerName);

            var dto = aCustomer.Adapt<CustomerDTO>(config);

            Assert.IsNotNull(dto);
            dto.Id.ShouldBe(customerId);
            dto.Name.ShouldBe(customerName);
        }

        [TestMethod]
        public void Test_Dictionary()
        {
            var config = new TypeAdapterConfig();
            config.ForType<IDictionary<string, object>, Pet>()
                .EnableNonPublicMembers(true);

            var pet = new Dictionary<string, object>
            {
                { "Name", "Fluffy" }, 
                { "Color", "White" }
            }.Adapt<Pet>(config);

            pet.Name.ShouldBe("Fluffy");
            pet.GetPrivateColor().ShouldBe("White");
        }

        private static void SetUpMappingNonPublicFields<TSource, TDestination>()
        {
            TypeAdapterConfig<TSource, TDestination>
                .NewConfig()
                .EnableNonPublicMembers(true)
                .NameMatchingStrategy(NameMatchingStrategy.Flexible);
        }

        private static void SetUpMappingNonPublicProperties<TSource, TDestination>()
        {
            TypeAdapterConfig<TSource, TDestination>
                  .NewConfig()
                  .EnableNonPublicMembers(true);
        }

        #region TestMethod Classes

        public class CustomerWithPrivateField // this detect as Record 
        {
            private readonly int _id; // this not taken into account no public getter
            public string Name { get; private set; }

            private CustomerWithPrivateField() { } // not public constructor not takened 

            public CustomerWithPrivateField(int id, string name) // two is two == true this worked as record
            {
                _id = id;
                Name = name;
            }

            public bool HasId(int id) // it is not property  
            {
                return _id == id;
            }
        }

        public class CustomerWithPrivateProperty // this detect as Record 
        {
            public int Id { get; private set; }
            private string Name { get; set; }

            private CustomerWithPrivateProperty() { } // not public constructor is not taken 

            public CustomerWithPrivateProperty(int id, string name) // two is two == true this worked as record
            {
                Id = id;
                Name = name;
            }

            public bool HasName(string name)  // it is not property  
            {
                return Name == name;
            }
        }

        public class CustomerWithProtectedProperty
        {
            public int Id { get; private set; }
            protected string Name { get; set; }

            private CustomerWithProtectedProperty() { }

            public CustomerWithProtectedProperty(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public bool HasName(string name)
            {
                return Name == name;
            }
        }

        public class CustomerDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Pet
        {
            public string Name { get; set; }

            private string Color { get; set; }

            public string GetPrivateColor()
            {
                return this.Color;
            }
        }
        #endregion
    }
}
