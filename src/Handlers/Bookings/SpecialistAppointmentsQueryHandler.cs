﻿using AutoMapper;
using MedPark.Bookings.Domain;
using MedPark.Bookings.Dto;
using MedPark.Bookings.Queries;
using MedPark.Common;
using MedPark.Common.Handlers;
using MedPark.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedPark.Bookings.Handlers.Bookings
{
    public class SpecialistAppointmentsQueryHandler : IQueryHandler<SpecialistAppointmentQuery, SpecialistAppointmentsDto>
    {
        private IMedParkRepository<Appointment> _bookingsRepo;
        private IMedParkRepository<Specialist> _specialistRepo;
        private IMedParkRepository<Patient> _patientRepo;
        private IMapper _mapper;

        public SpecialistAppointmentsQueryHandler(IMedParkRepository<Appointment> bookingsRepo, IMedParkRepository<Specialist> specialistRepo, IMapper mapper, IMedParkRepository<Patient> patientRepo)
        {
            _bookingsRepo = bookingsRepo;
            _specialistRepo = specialistRepo;
            _mapper = mapper;
            _patientRepo = patientRepo;
        }

        public async Task<SpecialistAppointmentsDto> HandleAsync(SpecialistAppointmentQuery query)
        {
            Specialist doctor = await _specialistRepo.GetAsync(query.SpecialistId);

            if (doctor is null)
                throw new MedParkException("bookings_specialist_does_not_exist", $"Specialist with Id {query.SpecialistId} does not exist.");

            IEnumerable<Appointment> bookings = await _bookingsRepo.FindAsync(x => x.SpecialistId == query.SpecialistId && x.ScheduledDate >= DateTime.Today);

            SpecialistAppointmentsDto result = new SpecialistAppointmentsDto();

            foreach (var b in bookings)
            {
                Patient patient = await _patientRepo.GetAsync(b.PatientId);

                SpecialistAppointmentDto app = _mapper.Map<SpecialistAppointmentDto>(patient);
                app.Id = b.Id;
                app.PatientId = patient.Id;
                app.ScheduledDate = b.ScheduledDate;
                app.FirstName = patient.FirstName;
                app.LastName = patient.LastName;
                app.Email = patient.Email;

                result.BookingDetails.Add(app);
            };

            return result;
        }
    }
}
