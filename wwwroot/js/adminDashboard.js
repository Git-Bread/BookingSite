// Time slot management
function attachTimeSlotHandlers() {
    document.querySelectorAll('.toggle-timeslot').forEach(button => {
        // Remove existing listeners to prevent duplicates
        button.replaceWith(button.cloneNode(true));
        const newButton = document.querySelector(`[data-timeslot-id="${button.dataset.timeslotId}"]`);
        
        newButton.addEventListener('click', async function() {
            const timeSlotId = this.dataset.timeslotId;
            const currentState = this.dataset.currentState === 'true';
            const newState = !currentState;

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                const response = await fetch('/Admin/UpdateTimeSlot', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: `timeSlotId=${timeSlotId}&isEnabled=${newState}`
                });

                const result = await response.json();
                console.log('Update result:', result);

                if (result.success) {
                    // Update button appearance
                    this.classList.toggle('btn-outline-success');
                    this.classList.toggle('btn-outline-danger');
                    this.dataset.currentState = newState.toString();
                    
                    // Clear the button content
                    this.innerHTML = '';
                    
                    // Add icon
                    const icon = document.createElement('i');
                    icon.className = `fas ${newState ? 'fa-ban' : 'fa-check'}`;
                    this.appendChild(icon);
                    
                    // text
                    this.appendChild(document.createTextNode(` ${newState ? 'Disable' : 'Enable'}`));

                    // Update status badge
                    const badge = this.closest('tr').querySelector('.badge');
                    badge.classList.toggle('bg-success');
                    badge.classList.toggle('bg-secondary');
                    badge.textContent = newState ? 'Enabled' : 'Disabled';
                } else {
                    alert('Failed to update time slot status');
                }
            } catch (error) {
                console.error('Error updating time slot:', error);
                alert('Failed to update time slot. Please try again.');
            }
        });
    });
}

// Room creation
document.getElementById('saveRoom')?.addEventListener('click', async function() {
    const name = document.getElementById('roomName').value;
    const location = document.getElementById('roomLocation').value;
    const openDays = Array.from(document.querySelectorAll('input[type="checkbox"]:checked'))
        .map(cb => parseInt(cb.value));

    try {
        const response = await fetch('/Admin/CreateRoom', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ name, location, openDays })
        });

        const result = await response.json();
        if (result.success) {
            window.location.reload();
        }
    } catch (error) {
        console.error('Error creating room:', error);
    }
});

// Time slot management button handling
document.querySelectorAll('.manage-timeslots').forEach(button => {
    button.addEventListener('click', async function() {
        const roomId = this.dataset.roomId;
        const container = document.getElementById('timeSlotManagementContainer');

        // Remove selected class from all cards
        document.querySelectorAll('.room-card').forEach(card => {
            card.classList.remove('selected');
        });

        // Add selected class to current card
        this.closest('.room-card').classList.add('selected');

        try {
            const response = await fetch(`/Admin/GetRoomTimeSlots?roomId=${roomId}`);
            const html = await response.text();
            container.innerHTML = html;
            container.scrollIntoView({ behavior: 'smooth' });
            
            // Attach event handlers to the new loaded time slot buttons
            attachTimeSlotHandlers();
        } catch (error) {
            console.error('Error loading time slots:', error);
        }
    });
});

// Delete room functionality
document.querySelectorAll('.delete-room').forEach(button => {
    button.addEventListener('click', async function() {
        const roomId = this.dataset.roomId;
        const roomName = this.dataset.roomName;
        
        if (!confirm(`Are you sure you want to delete the room "${roomName}"? This will also delete all associated time slots and bookings.`)) {
            return;
        }

        try {
            const response = await fetch('/Admin/DeleteRoom', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: `roomId=${roomId}`
            });

            const result = await response.json();
            if (result.success) {
                // Remove the room card from the UI
                this.closest('.col-md-6').remove();
                // Clear time slots if this was the selected room
                const timeSlotContainer = document.getElementById('timeSlotManagementContainer');
                if (timeSlotContainer.querySelector(`[data-room-id="${roomId}"]`)) {
                    timeSlotContainer.innerHTML = '';
                }
            } else {
                alert('Failed to delete room: ' + (result.message || 'Unknown error'));
            }
        } catch (error) {
            console.error('Error deleting room:', error);
            alert('Failed to delete room. Please try again.');
        }
    });
});

// Edit room days functionality
let currentEditRoomId = null;

document.querySelectorAll('.edit-room-days').forEach(button => {
    button.addEventListener('click', function() {
        const roomId = this.dataset.roomId;
        const roomName = this.dataset.roomName;
        const openDays = this.dataset.openDays.split(',').map(Number);
        
        currentEditRoomId = roomId;
        document.getElementById('editRoomName').textContent = roomName;
        
        // Reset all checkboxes
        document.querySelectorAll('.edit-day').forEach(checkbox => {
            checkbox.checked = openDays.includes(parseInt(checkbox.value));
        });
    });
});

document.getElementById('saveRoomDays')?.addEventListener('click', async function() {
    if (!currentEditRoomId) return;

    const openDays = Array.from(document.querySelectorAll('.edit-day:checked'))
        .map(cb => parseInt(cb.value));

    if (openDays.length === 0) {
        alert('Please select at least one open day.');
        return;
    }

    try {
        const response = await fetch('/Admin/UpdateRoomDays', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ roomId: currentEditRoomId, openDays: openDays })
        });

        const result = await response.json();
        if (result.success) {
            window.location.reload(); // Reload to show updated days
        } else {
            alert('Failed to update room days: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        console.error('Error updating room days:', error);
        alert('Failed to update room days. Please try again.');
    }
});

// Booking management functionality
function initializeBookingManagement() {
    const bookingsTable = document.getElementById('bookingsTable');
    if (!bookingsTable) return;

    const bookingSearch = document.getElementById('bookingSearch');
    const dateFilter = document.getElementById('dateFilter');
    const clearSearch = document.getElementById('clearSearch');
    const clearDate = document.getElementById('clearDate');

    function filterBookings() {
        const searchTerm = bookingSearch.value.toLowerCase();
        const selectedDate = dateFilter.value;
        const rows = bookingsTable.getElementsByTagName('tbody')[0].getElementsByTagName('tr');

        for (let row of rows) {
            const roomName = row.cells[0].textContent.toLowerCase();
            const userEmail = row.cells[1].textContent.toLowerCase();
            const date = row.cells[2].textContent;
            const time = row.cells[3].textContent;
            const bookedOn = row.cells[4].textContent;

            const matchesSearch = roomName.includes(searchTerm) || 
                                userEmail.includes(searchTerm) || 
                                date.toLowerCase().includes(searchTerm) || 
                                time.toLowerCase().includes(searchTerm) ||
                                bookedOn.toLowerCase().includes(searchTerm);

            const matchesDate = !selectedDate || date.includes(selectedDate);

            row.style.display = matchesSearch && matchesDate ? '' : 'none';
        }
    }

    bookingSearch?.addEventListener('input', filterBookings);
    dateFilter?.addEventListener('change', filterBookings);
    clearSearch?.addEventListener('click', () => {
        bookingSearch.value = '';
        filterBookings();
    });
    clearDate?.addEventListener('click', () => {
        dateFilter.value = '';
        filterBookings();
    });

    // Cancel booking functionality
    let currentBookingId = null;
    const cancelModal = new bootstrap.Modal(document.getElementById('cancelBookingModal'));

    document.querySelectorAll('.cancel-booking').forEach(button => {
        button.addEventListener('click', function() {
            currentBookingId = this.dataset.bookingId;
            document.getElementById('cancelRoomName').textContent = this.dataset.roomName;
            document.getElementById('cancelUserEmail').textContent = this.dataset.userEmail;
            document.getElementById('cancelDate').textContent = this.dataset.date;
            document.getElementById('cancelTime').textContent = this.dataset.time;
            cancelModal.show();
        });
    });

    document.getElementById('confirmCancel')?.addEventListener('click', async function() {
        if (!currentBookingId) return;

        try {
            const response = await fetch('/Admin/CancelBooking', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: currentBookingId })
            });

            const result = await response.json();
            if (result.success) {
                window.location.reload();
            } else {
                alert('Failed to cancel booking: ' + (result.message || 'Unknown error'));
            }
        } catch (error) {
            console.error('Error canceling booking:', error);
            alert('Failed to cancel booking. Please try again.');
        }
    });
}

// Initialize all functionality when the DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    attachTimeSlotHandlers();
    initializeBookingManagement();
}); 