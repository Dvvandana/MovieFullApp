using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MovieFullApp.Data;
using MovieFullApp.Models;

namespace MovieFullApp.Controllers
{
    public class NewMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInMgr;
        private readonly UserManager<IdentityUser> _userMgr;


        private readonly IConfiguration _configuration;
        static CloudBlobClient _blobClient;
        const string blobContainerName = "imagecontainernew";
        static CloudBlobContainer _blobContainer;

        public NewMoviesController(ApplicationDbContext context, UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr, IConfiguration config)
        {
            _context = context;
            _signInMgr = signInMgr;
            _userMgr = userMgr;
            _configuration = config;
            SetUPBlob();
        }

        // GET: NewMovies
        public async Task<IActionResult> Index()
        {
            if (_signInMgr.IsSignedIn(User))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var movies = from m in _context.NewMovie select m;
                movies.Where(each_m => each_m.UserId == userId);

                return View(await movies.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        private async void SetUPBlob()
        {
            var storageConnectionString = _configuration.GetValue<string>("StorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            _blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(blobContainerName);
            await _blobContainer.CreateIfNotExistsAsync();
            await _blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

        }

        // GET: NewMovies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newMovie = await _context.NewMovie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (newMovie == null)
            {
                return NotFound();
            }

            return View(newMovie);
        }

        // GET: NewMovies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NewMovies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Rating,IsPublic,File")] NewMovie newMovie)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                newMovie.UserId = userId;

                //Save  the Image in blob Storage
                CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(GetRandomBlobName(newMovie.File.FileName));
                using (var stream = newMovie.File.OpenReadStream())
                {
                    await blob.UploadFromStreamAsync(stream);
                }
                newMovie.ImageName = newMovie.Title + "_Img";
                newMovie.ImagePath = blob.Uri.AbsoluteUri;

                _context.Add(newMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(newMovie);
        }
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }

        // GET: NewMovies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newMovie = await _context.NewMovie.FindAsync(id);
            if (newMovie == null)
            {
                return NotFound();
            }
            return View(newMovie);
        }

        // POST: NewMovies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Rating,IsPublic,UserId,ImageName,ImagePath")] NewMovie newMovie)
        {
            if (id != newMovie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(newMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewMovieExists(newMovie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(newMovie);
        }

        // GET: NewMovies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newMovie = await _context.NewMovie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (newMovie == null)
            {
                return NotFound();
            }

            return View(newMovie);
        }

        // POST: NewMovies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var newMovie = await _context.NewMovie.FindAsync(id);
            _context.NewMovie.Remove(newMovie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewMovieExists(int id)
        {
            return _context.NewMovie.Any(e => e.Id == id);
        }
    }
}
