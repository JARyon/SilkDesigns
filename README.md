# SilkDesigns
Silk Designs Flower Rotation System
To DO:
=======
Major Functionality

1) Fix bug which sets inventory status to Allocated when initially assigning an inventory item to a location.

2) Multi User Support Review the solution for allowing multiple franchises to use this system. This can be done with logically separating the data by your login, or we could physically have new dbs for each franchise.

However, in either scenario, would like to be able to define security levels for logins associated with a franchise regardless of database decision. Different logins have access to different functionality within the system.

3) Add truck to route and ability to transfer inventory to truck on a route Sometimes additional flower arrangements may be placed into the truck as an emergency replacement arrangement. The system has no method to track that an arrangement has been placed on the truck and is not available for use elsewhere, as its physically not available.

4) Add ability to transfer from truck to warehouse, or replace in warehouse when plan is finalized If a truck can have an emergency arrangement in it, we need to be able to put that arrangement back into the warehouse so it can be used by other routes.

5) Tie Warehouse by routes? Check w/ Michael. Should a route be tied to a particular warehouse. If warehouses are geographically separated, it may make sense to allow a route to only pull from the closest warehouse (ie the one assigned to the route).

6) Add Arrangement attributes (Seasonal, color etc) Currently the system only considers a couple of things when making the selection during a plan generation. It uses the following:
  History; How long has it been since this arrangement was seen at the location
  Is the arrangement of the proper size.
  Is the arrangement in the proper status (Available, and deleted = 'N')
  This change would involve adding additional attributes to the Arrangements that would be considered in the plan generation process. Below are SOME of the       ideas/suggestion for new attributes:

  Arrangement Style = Traditional or Modern (Suggested/Asked for by Scott)
  Seasonal - Valentines, Christmas, Winter, Summer, Fall, Spring etc.
  
7) Allow adding of history input/import. For onboarding of a franchise, they may have historical data for arrangement history. This screen would allow for the input of that data so delivery of recently viewed arrangements can be avoided.

8) Remove the Success Message when creating a customer and rediret to customer list.
9) When creating a customer, it automatically creates a location.  change the name of that default location from "Default customer location" to be the name of the customer.

========  Other To Do  ========

- Decide if db trigger should be used to update inventory quantity. - Inventory quantity will be updated via code, as a trigger would have to read the ArrangementInventoryTable to get the proper count, causing a error when trying to read from the same table that the trigger is fired from.

- The only other option is for the trigger to increment the quantity (+ or -1) when the record is inserted or marked for deletion. This would also require the the Create Arrangement screen would have to be modified to NOT write the qty to the Arrangement Table. Thus when the newly created inventory is assigned a location, the quantity would then be incremented.

- Filter Placements by size

- Update Inventory History when a Inventory Item is assigned to a customer location via the arrangement Inventory create/update screens.

- Set routes so that a customer location may only appear 1 time in a route. (Can be on different routes though)

- Add ability to update arrangements from the location screen. To put an arrangement in a location or to update a location/placement's arrangement.

- Create trigger on RoutePlanDetailRecords when deleted, reset the inventory status of the Arrangement Inventory back to available?

- Add location to the Create Arrangement Screen

- Add placements to the Warehouse

=========  Questions  ===============

1)  When updating an arrangement via The arrangement/inventory update add screen, should we stop the user from assigning it to a location/placement/size if there are no available placements of the same size?

2)  Is is possible that a location would show up under multiple delivery Routes? YES - from Michael 2/8/23

3)  Ie. would it be possible that a customer would have an arrangements delivered on different trips. Example. Customer A has 2 arrangements; One is Roses, the other Tulips. Would there ever be a time where, on a regular basis, that one delivery would swap out the roses, and then a seperate delivery would swap out the tulips. 1 customer, 2 arrangements swapped out on different routes? Currently I do not support that.

4)  Should I color code the items in the plan that are being transferred to make it easier to tell where they are from and where they are going?

==========  Completed  ============

- Add support for a quantity > 1 for a placement/size
- Create inventory history trigger. on Insert, update of Location Inventory table
- Change Arrangement Catalog to Catalog, and update menu to Catalog
- Cancel a plan
- When assigning arrangement to a location via the Update Arrangement screen, the inventory status needs to be set to InUse when going to a customer location, and set to Available when going to a warehouse.
- Add images to inventory
- Finalize a plan
- Change Update title and link to Edit and Pencil icon
- Remove delete button from list on the Routs list.
- Add delete button to Customer Edit screen.
- Move the update button up from bottom of page to below the input fields.
- remove deleted inventory from lists

======  Validations =======
- Which style of validation do you want: See Create Route vs Create Size. -Michael either one is fine. So I decided to have the validation fields on the screen and check if Model.IsValid.

- Which style of selection do you want: See Routes vs Customers - Edit Icon was selected.
