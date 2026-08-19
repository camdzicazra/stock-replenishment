using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using StockReplenishment.Data;
using StockReplenishment.Models;
using StockReplenishment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockReplenishment.Tests;

[TestFixture]
public class ReplenishmentRequestServiceTests
{
    private AppDbContext _context;
    private ReplenishmentRequestService _service;
    private IServiceScopeFactory _scopeFactoryMock;
    private IServiceProvider _serviceProviderMock;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new AppDbContext(options);

        _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        var scopeMock = Substitute.For<IServiceScope>();
        _serviceProviderMock = Substitute.For<IServiceProvider>();
        
        _scopeFactoryMock.CreateScope().Returns(scopeMock);
        scopeMock.ServiceProvider.Returns(_serviceProviderMock);
        _serviceProviderMock.GetService(typeof(AppDbContext)).Returns(_context);

        _service = new ReplenishmentRequestService(_context, _scopeFactoryMock);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task CreateDraftAsync_WithValidItems_SavesAsDraft()
    {
        var newRequest = new ReplenishmentRequest
        {
            TargetLocation = "Warehouse A",
            Items = new List<RequestItem> 
            { 
                new RequestItem { ArticleNumber = "ART-1", Description = "Test Item", RequestedQuantity = 10 } 
            }
        };

        var result = await _service.CreateDraftAsync(newRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(RequestStatus.Draft));
        Assert.That(_context.Requests.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task SubmitRequestAsync_WhenInDraft_UpdatesToSubmitted()
    {
        var draft = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Draft };
        _context.Requests.Add(draft);
        await _context.SaveChangesAsync();

        var result = await _service.SubmitRequestAsync(draft.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(RequestStatus.Submitted));
    }

    [Test]
    public async Task ApproveRequestAsync_WhenSubmitted_UpdatesToApproved()
    {
        var submitted = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Submitted };
        _context.Requests.Add(submitted);
        await _context.SaveChangesAsync();

        var result = await _service.ApproveRequestAsync(submitted.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(RequestStatus.Approved));
        Assert.That(result.RejectionReason, Is.Null);
    }

    [Test]
    public async Task RejectRequestAsync_WhenSubmitted_UpdatesToRejectedWithReason()
    {
        var submitted = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Submitted };
        _context.Requests.Add(submitted);
        await _context.SaveChangesAsync();

        var result = await _service.RejectRequestAsync(submitted.Id, "Budget exceeded");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(RequestStatus.Rejected));
        Assert.That(result.RejectionReason, Is.EqualTo("Budget exceeded"));
    }

    [Test]
    public async Task FulfillRequestAsync_WhenApproved_SavesFulfilledQuantities()
    {
        var approved = new ReplenishmentRequest 
        { 
            TargetLocation = "Loc A", 
            Status = RequestStatus.Approved,
            Items = new List<RequestItem> 
            { 
                new RequestItem { ArticleNumber = "ART-1", Description = "Screws", RequestedQuantity = 100 } 
            }
        };
        _context.Requests.Add(approved);
        await _context.SaveChangesAsync();

        var fulfillmentPayload = new List<ItemFulfillment> 
        { 
            new ItemFulfillment { ArticleNumber = "ART-1", FulfilledQuantity = 80 } 
        };

        var result = await _service.FulfillRequestAsync(approved.Id, fulfillmentPayload);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(RequestStatus.Fulfilled));
        Assert.That(result.Items.First().FulfilledQuantity, Is.EqualTo(80));
    }

    [Test]
    public async Task SubmitRequestAsync_WhenAlreadySubmitted_ReturnsNull()
    {
        var submitted = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Submitted };
        _context.Requests.Add(submitted);
        await _context.SaveChangesAsync();

        var result = await _service.SubmitRequestAsync(submitted.Id);

        Assert.That(result, Is.Null, "Should not allow submitting an already submitted request.");
    }

    [Test]
    public async Task ApproveRequestAsync_WhenStatusIsDraft_ReturnsNull()
    {
        var draft = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Draft };
        _context.Requests.Add(draft);
        await _context.SaveChangesAsync();

        var result = await _service.ApproveRequestAsync(draft.Id);

        Assert.That(result, Is.Null, "Cannot approve a Draft. It must be Submitted first.");
    }

    [Test]
    public async Task FulfillRequestAsync_WhenStatusIsRejected_ReturnsNull()
    {
        var rejected = new ReplenishmentRequest { TargetLocation = "Loc A", Status = RequestStatus.Rejected };
        _context.Requests.Add(rejected);
        await _context.SaveChangesAsync();

        var payload = new List<ItemFulfillment>();

        var result = await _service.FulfillRequestAsync(rejected.Id, payload);

        Assert.That(result, Is.Null, "Cannot fulfill a rejected request.");
    }

    [Test]
    public async Task FulfillRequestAsync_WithMissingItemsInPayload_SafelyIgnoresThem()
    {
        var approved = new ReplenishmentRequest 
        { 
            TargetLocation = "Loc A",
            Status = RequestStatus.Approved,
            Items = new List<RequestItem> 
            { 
                new RequestItem { ArticleNumber = "ART-1", Description = "Item 1", RequestedQuantity = 10 },
                new RequestItem { ArticleNumber = "ART-2", Description = "Item 2", RequestedQuantity = 10 }
            }
        };
        _context.Requests.Add(approved);
        await _context.SaveChangesAsync();

        var partialPayload = new List<ItemFulfillment> 
        { 
            new ItemFulfillment { ArticleNumber = "ART-1", FulfilledQuantity = 5 } 
        };

        var result = await _service.FulfillRequestAsync(approved.Id, partialPayload);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.First(i => i.ArticleNumber == "ART-1").FulfilledQuantity, Is.EqualTo(5));
        Assert.That(result.Items.First(i => i.ArticleNumber == "ART-2").FulfilledQuantity, Is.Null); 
    }

    [Test]
    public async Task SubmitRequestAsync_WhenIdDoesNotExist_ReturnsNull()
    {
        var result = await _service.SubmitRequestAsync(99999);
        Assert.That(result, Is.Null);
    }
}