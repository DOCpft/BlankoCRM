import { FC, useMemo, useState } from 'react';
import { Button, Div, Group, NavIdProps, Panel, PanelHeader } from '@vkontakte/vkui';

import { EntityFormModal } from '../components/EntityFormModal';
import { SlotChoiceModal } from '../components/SlotChoiceModal';
import { WeekCalendar } from '../components/WeekCalendar';
import { useCrmStore } from '../store/useCrmStore';
import { Slot } from '../types';
import {
  getDateTimeInputValue,
  getWeekStart,
  parseDateTimeInputValue,
} from '../utils/dateUtils';

export interface SlotsProps extends NavIdProps {}

export const Slots: FC<SlotsProps> = ({ id }) => {
  const [weekStart, setWeekStart] = useState(() => getWeekStart(new Date()));
  const [choiceDate, setChoiceDate] = useState<Date | null>(null);
  const [draftDate, setDraftDate] = useState<Date | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<Slot | null>(null);
  const [isSlotFormOpen, setIsSlotFormOpen] = useState(false);
  const slots = useCrmStore((state) => state.slots);
  const clients = useCrmStore((state) => state.clients);
  const services = useCrmStore((state) => state.services);
  const addSlot = useCrmStore((state) => state.addSlot);
  const updateSlot = useCrmStore((state) => state.updateSlot);
  const deleteSlot = useCrmStore((state) => state.deleteSlot);

  const weekSlots = useMemo(
    () => slots.filter((slot) => slot.startTime < new Date(weekStart.getTime() + 7 * 86400000) && slot.endTime > weekStart),
    [slots, weekStart],
  );

  const closeSlotForm = () => {
    setIsSlotFormOpen(false);
    setSelectedSlot(null);
    setDraftDate(null);
  };

  const handleCreateEmptySlot = () => {
    if (!choiceDate) return;
    const endTime = new Date(choiceDate.getTime() + 30 * 60000);
    addSlot({ startTime: choiceDate, endTime, isFree: false });
    setChoiceDate(null);
  };

  const handleCreateAppointment = () => {
    setDraftDate(choiceDate);
    setIsSlotFormOpen(true);
    setChoiceDate(null);
  };

  const handleSlotSubmit = (values: Record<string, string>) => {
    const startTime = parseDateTimeInputValue(values.startTime);
    const endTime = parseDateTimeInputValue(values.endTime);
    const slotData = {
      startTime,
      endTime,
      clientId: values.clientId || undefined,
      serviceId: values.serviceId || undefined,
      isFree: false,
    };

    if (selectedSlot) {
      updateSlot(selectedSlot.id, slotData);
    } else {
      addSlot(slotData);
    }
    closeSlotForm();
  };

  const formInitialValues: Record<string, string> = selectedSlot
    ? {
        startTime: getDateTimeInputValue(selectedSlot.startTime),
        endTime: getDateTimeInputValue(selectedSlot.endTime),
        clientId: selectedSlot.clientId || '',
        serviceId: selectedSlot.serviceId || '',
      }
    : draftDate
      ? {
          startTime: getDateTimeInputValue(draftDate),
          endTime: getDateTimeInputValue(new Date(draftDate.getTime() + 30 * 60000)),
          clientId: '',
          serviceId: '',
        }
      : {};

  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Слоты</div>
      </PanelHeader>
      <Div className="slots-toolbar">
        <Button size="m" mode="secondary" onClick={() => setWeekStart((date) => new Date(date.getTime() - 7 * 86400000))}>
          ←
        </Button>
        <strong>{weekStart.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })}</strong>
        <Button size="m" mode="secondary" onClick={() => setWeekStart((date) => new Date(date.getTime() + 7 * 86400000))}>
          →
        </Button>
      </Div>
      <Group>
        <Div>
          <WeekCalendar
            weekStart={weekStart}
            slots={weekSlots}
            clients={clients}
            services={services}
            onSlotClick={setChoiceDate}
            onSlotSelect={(slot) => {
              setSelectedSlot(slot);
              setIsSlotFormOpen(true);
            }}
          />
        </Div>
      </Group>
      <SlotChoiceModal
        active={choiceDate !== null}
        onClose={() => setChoiceDate(null)}
        onCreateAppointment={handleCreateAppointment}
        onCreateEmptySlot={handleCreateEmptySlot}
      />
      {(isSlotFormOpen || choiceDate) && (
        <EntityFormModal
          key={selectedSlot?.id || draftDate?.toISOString() || 'new-slot'}
          id="slot-form"
          title={selectedSlot ? 'Редактировать запись' : 'Новая запись'}
          active={isSlotFormOpen}
          initialValues={formInitialValues}
          onClose={closeSlotForm}
          onSubmit={handleSlotSubmit}
          onDelete={selectedSlot ? () => { deleteSlot(selectedSlot.id); closeSlotForm(); } : undefined}
          fields={[
            { name: 'startTime', label: 'Начало', type: 'datetime-local', required: true },
            { name: 'endTime', label: 'Окончание', type: 'datetime-local', required: true },
            {
              name: 'clientId',
              label: 'Клиент',
              type: 'select',
              required: !selectedSlot?.isFree,
              options: [{ value: '', label: 'Выберите клиента' }, ...clients.map((client) => ({ value: client.id, label: client.name }))],
            },
            {
              name: 'serviceId',
              label: 'Услуга',
              type: 'select',
              required: !selectedSlot?.isFree,
              options: [{ value: '', label: 'Выберите услугу' }, ...services.map((service) => ({ value: service.id, label: service.name }))],
            },
          ]}
        />
      )}
    </Panel>
  );
};

export default Slots;
