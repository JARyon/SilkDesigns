# SilkDesigns
Silk Designs Flower Rotation System
To DO:
=======
Major Functionality
0) Fix bug which sets inventory status to Allocated when initially assigning an inventory item to a location.

1) Multi User Support
   Review the solution for allowing multiple franchises to use this system.  This can be done with logically 
   seperating the data by your login, or we could physically have new dbs for each franchise.
   
   However, in either scenario, would like to be able to define security levels for logins assocaiated with a 
   franchise regardless of database decision. Different logins have access to different functionality within
   the system.
   
2) Add truck to route and ability to transfer inventory to truck on a route
   Sometimes additional flower arrangements may be placed into the truck as an emergency replacment arrangment.
   The system has no method to track that an arrangment has been placed on the truck and is not available for use
   elsewhere, as its physically not available.
   
3) Add ability to transfer from truck to warehouse, or replace in warehouse when plan is finalized
	If a truck can have an emergency arrangement in it, we need to be able to put that arrangment back into the
	warehouse so it can be used by other routes.
	
4) Tie Warehouse by routes?  Check w/ Michael.
	Should a route be tied to a particular warehouse.  If warehouses are geographyically seperated, it may make
	sense to allow a route to only pull from the closest warehouse (ie the one assigned to the route).
	
7) Add Arrangment atttributes (Seasonal, color etc)
	Currently the system only considers a couple of things when making the selection during a plan generation. 
	It uses the following:
	1) History; How long has it been since this arrangment was seen at the location
	2) Is the arrangment of the proper size.
	3) Is the arrangmet in the proper status (Available, and deleted = 'N')
	
	This change would involve adding additional attributes to the Arrangments that would be considered in the 
	plan generation process. Below are SOME of the ideas/suggestion for new attributes:
	1)  ArrangementStyle = Traditional or Modern (Suggeste/Asked for by Scott)
	2)	Seasonal - Valenties, Christmas, Winter, Summer, Fall, Spring etc.

15) Allow adding of history input/import.
	For onboarding of a franchise, they may have historical data for arrangment history.  This screen
	would allow for the input of that data so delivery of recently viewed arrangments can be avoided.

Other ToDOs
===============
1) Decide if db trigger should be used to update inventory quantity. - Inventory quantity will be updated via code,
   as a trigger would have to read the ArrangementInventoryTable to get the proper count, causing a error when trying 
   to read from the same table that the trigger is fired from.
  
	The only other option  is for the trigger to increment the quantity (+ or -1) when the record is inserted or marked for 
	deltion. This would also require the the Create Arrangment screen would have to be modified to NOT write the qty
	to the Arrangment Table.  Thus when the newly created inventory is assigned a location, the quantity would then be 
	incremented.
  
2) Filter Placments by size
3) Update Inventory History when a Inventory Item is assigned to a customer location via the
   arrangement Inventory create/update screens.
4) Set routes so that a customer location may only appear 1 time in a route. (Can be on different routes though)
 
6) Add ability to update arrangments from the location screen. To put an arrangment in a location
   or to update a location/placment's arrangement.
7) Create trigger on RoutePlanDetailRecords when deleted, reset the inventory status of the ArrangmentInventory
   back to available?
9) Add location to the Create Arrangement Screen
10) Add placements to the Warehouse
13) Remove inactive invetory from list on the screen.

 Questions:
 When updating an arrangement via The arrangment/inventory update add screen, should we 
 stop the user from asigning it to a location/placement/size if there are no available placments
 of the same size?
 
 Is is possible that a location would show up under multiple delivery Routes?  YES - from Michael 2/8/23

 Ie.  would it be possible that a customer would
 have an arrangements delivered on different trips.  Example.  Customer A has 2 arrangments; One is Roses, the other Tulips.  Would there 
 ever be a time where, on a regular basis, that one delivery would swap out the roses, and then a seperate delivery would swap 
 out the tulips.  1 customer, 2 arrangments swapped out on differnt routes?  Currently I do not support that.
 
 Should I color code the items in the plan that are being transferred to make it easier to tell where they are from and
 where they are going?
 
 
 Completed:

1) Add support for a quantity > 1 for a placement/size
5) Create inventory history trigger. on INsert, update of Location Inventory table
8) Change Arrangement Catalog to Catalog, and update menu to Catalog
5) Cancel a plan
5) When assigning arrangment to a location via the Update Arrangement screen, the inventory status
   needs to be set to InUse when going to a customer location, and set to Available when going to a 
   warehouse.  
9) Add images to inventory
6) Finalize a plan
9) Change Update title and link to Edit and Pencil icon
11) Remove delete button from list on the Routs list.
10) Add delete button to Customer Edit screen.
12) Move the update button up from bottom of page to below the input fields.

 VALIDATIONS:
 Which style of validation do you want:  See Create Route vs Create Size. -Michael either one
 is fine. So I decided to have the validion fields on the screen and check if Model.IsValid.
 
 Which style of selection do you want:  See Routes vs Customers - Edit Icon was selected.
