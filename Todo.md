# Things to do

_Should be open issues_

- [x] paginated response for requests retrival ([GET]/api/requests)
- [x] endpoint to retrieve all info about a single book
- [x] endpoint to retrive info of a list of input bookIds
- [x] delete book endpoint
- [ ] Borrow Request DTO when sending to Reader should be contain number of books requested and not the actual book objects, to reduce size and improve speed

### 💡

- Maybe Book navigation path from Request shouldn't be popuated until books have been borrowed, so that we know which approved request has which books. A list of BookIds can be used to store the requested books until the request is approved and the Books property will then be populated with the borrowed books

- BorrowRequests should have a standardized ID format for easy identification (`#PD-2325`)

- Readers can get an email notification informing them of the new status of their submited request.
