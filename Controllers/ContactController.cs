using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ContactBook.CustomExceptions;
using ContactBook.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using ContactBook.Context;
using ContactBook.Models;

namespace Controllers
{

    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {

        private readonly ApplicationContext _applicationContext;
        private readonly UserManager<UserAplication> _userManager;

        public ContactController(ApplicationContext applicationContext, UserManager<UserAplication> userManager)
        {
            _applicationContext = applicationContext;
            _userManager = userManager;
        }

        /// <summary> Find contact by name. </summary>
        /// <returns> Returns a contacts page </returns>
        [Authorize]
        [HttpGet("find-by-name/{name}")]
        public ActionResult<PageResponse<List<Contact>>> GetContactByName([FromQuery] Pagination pagination, string name)
        {
            try
            {
                var userId = User?.Identity?.Name;

                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var contacts = _applicationContext.Contact
                    .Where(contact => contact.Name.Contains(name) && contact.UserAplicationId == userId)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var TotalRecords = _applicationContext.Contact
                    .Where(contact => contact.Name
                    .Contains(name) && contact.UserAplicationId == userId)
                    .Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}/api/contact/find-by-name/{name}";

                PageResponse<List<Contact>> pageResponse = new(
                    data: contacts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: TotalRecords,
                    uri: baseUri
                );

                return Ok(pageResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "Error finding contact!" });
            }
        }

        /// <summary> Get all contacts. </summary>
        /// <returns> Returns a contacts page </returns>
        [Authorize]
        [HttpGet]
        public ActionResult<PageResponse<List<Contact>>> GetAllContacts([FromQuery] Pagination pagination)
        {
            try
            {
                var userId = User?.Identity?.Name;

                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var contacts = _applicationContext.Contact
                    .Where(contact => contact.UserAplicationId == userId)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var TotalRecords = _applicationContext.Contact.Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}/api/contact";

                PageResponse<List<Contact>> pageResponse = new(
                    data: contacts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: TotalRecords,
                    uri: baseUri
                );

                return Ok(pageResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error geting contacts!" });
            }
        }

        /// <summary> Creates a contact. </summary>
        /// <returns> A newly created contact </returns>
        /// <response code="201"> Returns the newly created contact </response>
        /// <response code="400"> If any fields are missing or invalid </response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Contact>> PostContact(ContactDto contactDto)
        {
            try
            {
                string strModel = "^[\\d\\-+()?!]";
                if (!System.Text.RegularExpressions.Regex.IsMatch(contactDto.Telephone, strModel))
                {
                    return BadRequest(new { Message = "Telephone must be well-formed" });
                }

                var userLogged = await _userManager.FindByIdAsync(User.Identity.Name);

                Contact contact = new()
                {
                    Name = contactDto.Name,
                    Telephone = contactDto.Telephone,
                    IsActive = contactDto.IsActive,
                    UserAplicationId = userLogged.Id
                };

                _applicationContext.Contact.Add(contact);
                _applicationContext.SaveChanges();

                return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error creating contact!" });
            }

        }

        /// <summary> Get a contact by id. </summary>
        /// <returns> Returns a contact </returns>
        /// <response code="404"> If contact not exist </response>
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<Contact> GetContactById(int id)
        {
            try
            {
                var userId = User?.Identity?.Name;
                var contact = _applicationContext.Contact.Where(contact => contact.Id == id && contact.UserAplicationId == userId).FirstOrDefault();

                if (contact == null)
                {
                    return NotFound(new { Message = "Contact not found!" });
                }

                return Ok(contact);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error geting contact!" });
            }

        }

        /// <summary> Fully update a contact </summary>
        /// <returns> Returns the updated contact </returns>
        /// <response code="404"> If contact not exist </response>
        [HttpPut("{id}")]
        public ActionResult<Contact> PutContact(int id, ContactDto contactDto)
        {
            try
            {
                ContactDtoViewModel contactDtoViewModel = new()
                {
                    Name = contactDto.Name,
                    Telephone = contactDto.Telephone,
                    IsActive = contactDto.IsActive,
                };

                var contact = UpdateContact(id, contactDtoViewModel);

                return Ok(contact);
            }
            catch (NotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { ex.Message });
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error updating contact!" });
            }
        }

        /// <summary> Partially update a contact </summary>
        /// <returns> Returns the updated contact </returns>
        /// <response code="404"> If contact not exist </response>
        [HttpPatch("{id}")]
        public ActionResult<Contact> PatchContact(int id, ContactDtoViewModel contactDtoViewModel)
        {
            try
            {
                var contact = UpdateContact(id, contactDtoViewModel);

                return Ok(contact);
            }
            catch (NotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { ex.Message });
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error updating contact!" });
            }
        }

        /* Function to update contact, this function is used by PUT and PATCH */
        private Contact UpdateContact(int contactId, ContactDtoViewModel contactDtoViewModel)
        {

            string strModel = "^[\\d\\-+()?!]";
            if (!System.Text.RegularExpressions.Regex.IsMatch(contactDtoViewModel.Telephone, strModel))
            {
                throw new BadHttpRequestException("Telephone must be well-formed");
            }

            var userId = User?.Identity?.Name;
            var contact = _applicationContext.Contact.Where(contact => contact.Id == contactId && contact.UserAplicationId == userId).FirstOrDefault();

            if (contact == null)
            {
                throw new NotFoundException(message: $"Contact of id {contactId} is not found");
            }

            if (!string.IsNullOrEmpty(contactDtoViewModel.Name))
            {
                contact.Name = contactDtoViewModel.Name;
            }

            if (!string.IsNullOrEmpty(contactDtoViewModel.Telephone))
            {
                contact.Telephone = contactDtoViewModel.Telephone;
            }

            if (contactDtoViewModel?.IsActive != null)
            {
                contact.IsActive = contactDtoViewModel.IsActive;
            }

            _applicationContext.Contact.Update(contact);
            _applicationContext.SaveChanges();

            return contact;
        }

        /// <summary> Deletes a specific contact. </summary>
        /// <response code="204"> If contact deleted success </response>
        /// <response code="404"> If contact not exist </response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteContact(int id)
        {
            try
            {
                var userId = User?.Identity?.Name;
                var contact = _applicationContext.Contact.Where(contact => contact.Id == id && contact.UserAplicationId == userId).FirstOrDefault();

                if (contact == null)
                {
                    return NotFound(new { Message = "Contact not found!" });
                }

                _applicationContext.Contact.Remove(contact);
                _applicationContext.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error deleting contact!" });
            }
        }

    }
}